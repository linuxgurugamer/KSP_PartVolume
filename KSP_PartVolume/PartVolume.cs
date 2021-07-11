using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEngine;
using KSP_Log;
using KSP.Localization;

namespace KSP_PartVolume
{

    internal class PartModification
    {
        internal StringBuilder cfg;
        internal float packedVolume;
        internal bool delModuleCargoPart;

        internal PartModification(StringBuilder s, float v, bool d = false)
        {
            cfg = s;
            packedVolume = v;
            delModuleCargoPart = d;
        }
    }


    [KSPAddon(KSPAddon.Startup.MainMenu, false)]
    public partial class PartVolume : MonoBehaviour
    {
        internal static PartVolume Instance;

        public static Log Log;
        public static string VOL_CFG_FILE;
        public static string CFG_FILE;
        public static string KIFA;
        public string RES_BLACKLIST;
        private const string MODDIR = "KSP_PartVolume";
        internal const string MODID = "KSP_PartVolume";
        internal const string MODNAME = "KSP Part Volume";
        internal const string PARTBLACKLIST = "PARTVOLUME_BLACKLIST";

        bool visible = false;
        static bool RestartWindowVisible = false;
        List<string> resourceBlackList;
        List<String> partBlacklist;

        private void Awake()
        {
            Instance = this;
#if DEBUG
            Log = new Log("KSP_PartVolume", Log.LEVEL.INFO);
#else
            Log = new Log("KSP_PartVolume", Log.LEVEL.ERROR);
#endif
            VOL_CFG_FILE = KSPUtil.ApplicationRootPath + "GameData/partVolumes.cfg";
            CFG_FILE = KSPUtil.ApplicationRootPath + "GameData/" + MODDIR + "/PluginData/KSP_PartVolume.cfg";
            KIFA = KSPUtil.ApplicationRootPath + "GameData/KerbalInventoryForAll/AllowModPartsInStock.cfg";
            RES_BLACKLIST = KSPUtil.ApplicationRootPath + "GameData/" + MODDIR + "/PluginData/ResourceBlacklist.txt";

            var blacklistFile = File.ReadAllLines(RES_BLACKLIST);
            resourceBlackList = new List<string>(blacklistFile);
            partBlacklist = new List<string>();

            ConfigNode[] partBlacklistNodes = GameDatabase.Instance.GetConfigNodes(PARTBLACKLIST);
            foreach (var n in partBlacklistNodes)
            {
                var v = n.GetValues("blacklistPart");
                foreach (var v1 in v)
                    partBlacklist.Add(v1);
            }
            foreach (var p in partBlacklist)
                Log.Info("Part blacklisted: " + p);
        }

        public void Start()
        {
            Settings.LoadConfig();
            if (CheckForKIFA())
                return;
            Start2();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            List<AvailablePart> loadedParts = PartLoader.LoadedPartsList; // PartLoader.Instance.loadedParts;

            StringBuilder stringBuilder;

            bool fileExists = File.Exists(VOL_CFG_FILE);

            Log.Info("Finding Parts Volume....");
            using (List<AvailablePart>.Enumerator partEnumerator = loadedParts.GetEnumerator())
            {
                while (partEnumerator.MoveNext())
                {
                    AvailablePart current = partEnumerator.Current;

                    //
                    // Don't do the flag or any kerbalEVA
                    //

                    if (!partBlacklist.Contains(current.name) &&
                        (current.name.Length < 9 || current.name.Substring(0, 9) != "kerbalEVA"))
                    {
                        bool contains_ModuleCargoPart = false;
                        bool contains_KSPPartVolumeModule = false;

                        bool containsCrew = false;
                        bool isTank = false;
                        bool sizeTooBig = false;
                        bool isStock = false;

                        bool isRcsPart = false;
                        bool isEnginePart = false;
                        ConfigNode currentCargoPart = null;

                        string[] urlParts = current.partUrl.Split('/');

                        if (!Settings.doStock)
                        {
                            if (urlParts[0] == "Squad" || urlParts[0] == "SquadExpansion")
                                isStock = true;
                        }
                        var moduleNodes = current.partConfig.GetNodes("MODULE");
                        for (int i = 0; i < moduleNodes.Length; i++)
                        {
                            var name = moduleNodes[i].GetValue("name");

                            if (name == "ModuleCargoPart")
                            {
                                contains_ModuleCargoPart = true;
                                currentCargoPart = moduleNodes[i];
                            }
                            if (name == "KSPPartVolumeModule") contains_KSPPartVolumeModule = true;
                            if (name == "ModuleRCS" || name == "ModuleRCSFX") isRcsPart = true;
                            if (name == "ModuleEngines" || name == "ModuleEnginesFX") isEnginePart = true;

                            //  Check for manned
                            if (!Settings.manned)
                            {
                                int CrewCapacity = 0;
                                if (current.partConfig.TryGetValue("CrewCapacity", ref CrewCapacity))
                                {
                                    if (CrewCapacity > 0)
                                        containsCrew = true;
                                }
                            }
                        }
                        if (contains_KSPPartVolumeModule)
                            contains_ModuleCargoPart = false;
                        var resNodes = current.partConfig.GetNodes("RESOURCE");
                        float mass = 0;
                        current.partConfig.TryGetValue("mass", ref mass);
                        float totalResMass = 0;

                        if (!Settings.doTanks)
                        {
                            foreach (var resNode in resNodes)
                            {
                                var name = resNode.GetValue("name");

                                if (resourceBlackList.Contains(name))
                                    continue;

                                float maxAmount = 0;
                                resNode.TryGetValue("maxAmount", ref maxAmount);
                                var definition = PartResourceLibrary.Instance.GetDefinition(name);
                                if (definition != null)
                                {
                                    var density = definition.density;
                                    float resMass = maxAmount * density;
                                    totalResMass += resMass;
                                }
                            }

                            if (totalResMass > mass)
                                isTank = true;
                        }
                        stringBuilder = new StringBuilder();

                        Bounds bounds = default(Bounds);
                        foreach (Bounds rendererBound in PartGeometryUtil.GetRendererBounds((Part)current.partPrefab))
                            bounds.Encapsulate(rendererBound);

                        float vol = (float)(bounds.size.x * bounds.size.y * bounds.size.z) * 1000f;

                        if (vol > Settings.largestAllowablePart && Settings.limitSize)
                            sizeTooBig = true;

                        var adjVol = AdjustedVolume(current, vol, isEnginePart, isRcsPart, out float adj);

                        if (currentCargoPart != null)
                        {
                            Log.Info("currentCargoPart: " + current.name);
                            if (currentCargoPart.HasValue("packedVolume"))
                            {
                                string s = currentCargoPart.GetValue("packedVolume");
                                currentCargoPart.SetValue("packedVolume", adjVol.ToString("F0"));
                                Log.Info("currentCargoPart: packedVolume: " + s + ", newPackedVolume: " + adjVol);

                            }
                            else
                                Log.Error("packedVolume not found");
                        }


                        StringBuilder tmp = new StringBuilder();
                        tmp.AppendLine("// " + current.partUrl);
                        tmp.AppendLine(string.Format("// Bounding Box Size: {0} liters", vol));
                        tmp.AppendLine("// Volume adjustment: " + (adj * 100).ToString("F0") + "%");
                        if (isRcsPart)
                            tmp.AppendLine("// RCS module detected");
                        if (isEnginePart)
                            tmp.AppendLine("// Engine module detected");
                        tmp.AppendLine("//");


                        if (!containsCrew && !isTank && !sizeTooBig && !contains_ModuleCargoPart && !isStock)
                        {
                            stringBuilder.Append(tmp);
                            string partName = urlParts[urlParts.Length - 1];
                            string adjName = partName.Replace(' ', '?').Replace('(', '?').Replace(')', '?');
                            stringBuilder.AppendLine("@PART[" + adjName + "]:HAS[!MODULE[ModuleCargoPart]]:Final");
                            stringBuilder.AppendLine("{");
                            stringBuilder.AppendLine("    MODULE");
                            stringBuilder.AppendLine("    {");
                            stringBuilder.AppendLine("        name = ModuleCargoPart");
                            stringBuilder.AppendLine("        packedVolume = " + adjVol.ToString("F0"));
                            stringBuilder.AppendLine("    }");
                            stringBuilder.AppendLine("}");

                            RestartWindowVisible = true;

                            Part part = UnityEngine.Object.Instantiate(current.partPrefab);
                            part.gameObject.SetActive(value: false);
                            foreach (PartModule m in part.Modules)
                            {
                                if (m.moduleName == "ModuleCargoPart")
                                {
                                    var mcp = m as ModuleCargoPart;
                                    mcp.part = part;
                                    mcp.packedVolume = adjVol;

                                    for (int i = current.moduleInfos.Count - 1; i >= 0; --i)
                                    {
                                        AvailablePart.ModuleInfo info = current.moduleInfos[i];
                                        if (info.moduleName == Localizer.Format("#autoLOC_8002221")) // Cargo Part
                                        {
                                            try
                                            {
                                                info.info = mcp.GetInfo();
                                            }
                                            catch (Exception ex)
                                            {
                                                Log.Error("PartInfo.Start, Part: " + current.partUrl + ", Exception caught in ModuleCargoPart.GetInfo, exception: " + ex.Message + "\n" + ex.StackTrace);
                                                info.info = "KSP_PartVolume error";
                                            }
                                            break;
                                        }
                                    }

                                }
                            }
                            Destroy(part);
                        }
                        else
                        {
                            if (!fileExists)
                            {
                                stringBuilder.Append(tmp);
                                stringBuilder.AppendLine("//   Bypass reasons:");
                                if (containsCrew)
                                    stringBuilder.AppendLine("//      contains crew ");
                                if (isTank)
                                    stringBuilder.AppendLine("//      is tank");
                                if (sizeTooBig)
                                    stringBuilder.AppendLine("//      size exceeds largestAllowablePart: " + Settings.largestAllowablePart);
                                if (contains_ModuleCargoPart)
                                    stringBuilder.AppendLine("//      contains ModuleCargoPart");
                                if (isStock)
                                    stringBuilder.AppendLine("//      is Stock");
                                stringBuilder.AppendLine("//");

                                adjVol = -999;
#if true
                                current.partConfig.RemoveNode(currentCargoPart);
                                Part part = UnityEngine.Object.Instantiate(current.partPrefab);
                                part.gameObject.SetActive(value: false);

                                Statics.Check4DelModCargoPart(part);
                                Destroy(part);
#endif
                            }
                        }
                        if (!Statics.modifiedParts.ContainsKey(current.partUrl))
                            Statics.modifiedParts.Add(current.partUrl, new PartModification(stringBuilder, adjVol, adjVol == -999));
                        else
                            Log.Error("modifiedParts already contains: " + current.partUrl);
                        if (!fileExists)
                            stringBuilder.AppendLine("// ----------------------------------------------------------------------");

                    }
                }
            }

            stringBuilder = new StringBuilder();
            if (Statics.modifiedParts.Count > 0)
            {
                foreach (var d in Statics.modifiedParts)
                    stringBuilder.Append(d.Value.cfg.ToString());

                File.AppendAllText(VOL_CFG_FILE, stringBuilder.ToString());
            }

            stopwatch.Stop();
            Log.Info("File written to " + VOL_CFG_FILE);
            Log.Info(string.Format("Run in {0}ms", (object)stopwatch.ElapsedMilliseconds));
            //if (numCargoPartsAdded > 0)
            //    ShowWarning(numCargoPartsAdded);
        }

        bool CheckForKIFA()
        {
            if (File.Exists(KIFA))
            {
                ShowKIFAWarning("KerbalInventoryForAll/AllowModPartsInStock.cfg exists.\nKSP_PartVolume will not work if this file exists");
                return true;
            }
            return false;
        }



        private float AdjustedVolume(AvailablePart availPart, float vol, bool isEngine, bool isRcs, out float adj)
        {
            float num = Settings.filler;
            if (availPart.category == PartCategories.Science)
                num = Math.Max(num, Settings.scienceFiller);
            if (isEngine)
                num = Math.Max(num, Settings.engineFiller);
            if (isRcs)
                num = Math.Max(num, Settings.rcsFiller);
            adj = num;
            return (float)Math.Floor(vol * (1.0 + num) + 1.0);
        }



    }
}
