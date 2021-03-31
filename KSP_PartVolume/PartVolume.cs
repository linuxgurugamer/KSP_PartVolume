using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEngine;
using KSP_Log;

namespace KSP_PartVolume
{
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

        SortedDictionary<string, StringBuilder> modifiedParts = new SortedDictionary<string, StringBuilder>();
        int numCargoPartsAdded = 0;
        bool visible = false;
        List<string> blackList;

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
            blackList = new List<string>(blacklistFile);
        }

        public void Start()
        {
            Settings.LoadConfig();
            if (CheckForKIFA())
                return;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            List<AvailablePart> loadedParts = PartLoader.Instance.loadedParts;

            StringBuilder stringBuilder;

            Log.Info("Finding Parts Volume....");
            using (List<AvailablePart>.Enumerator partEnumerator = loadedParts.GetEnumerator())
            {
                while (partEnumerator.MoveNext())
                {
                    AvailablePart current = partEnumerator.Current;

                    //
                    // Don't do the flag or any kerbalEVA
                    //
                    if ((current.name.Length != 4 || current.name != "flag") &&
                        (current.name.Length < 9 || current.name.Substring(0, 9) != "kerbalEVA"))
                    {
                        bool isCargoPart = false;
                        bool isRcsPart = false;
                        bool isEnginePart = false;
                        ConfigNode currentCargoPart = null;

                        string[] urlParts = current.partUrl.Split('/');

                        if (!Settings.doStock)
                        {
                            if (urlParts[0] == "Squad" || urlParts[0] == "SquadExpansion")
                                continue;
                        }
                        var moduleNodes = current.partConfig.GetNodes("MODULE");
                        for (int i = 0; i < moduleNodes.Length; i++)
                        {
                            var name = moduleNodes[i].GetValue("name");
                            if (name == "ModuleCargoPart")
                                isCargoPart = true;

                            if (name == "ModuleRCS" || name == "ModuleRCSFX") isRcsPart = true;
                            if (name == "ModuleEngines" || name == "ModuleEnginesFX") isEnginePart = true;

                            //  Check for manned
                            if (!Settings.manned)
                            {
                                int CrewCapacity = 0;
                                if (current.partConfig.TryGetValue("CrewCapacity", ref CrewCapacity))
                                {
                                    if (CrewCapacity > 0)
                                        continue;
                                }
                            }
                        }
                        var resNodes = current.partConfig.GetNodes("RESOURCE");
                        float mass = 0;
                        current.partConfig.TryGetValue("mass", ref mass);
                        float totalResMass = 0;

                        if (!Settings.doTanks)
                        {
                            foreach (var resNode in resNodes)
                            {
                                var name = resNode.GetValue("name");
                                if (blackList.Contains(name))
                                    continue;
                                float maxAmount = 0;
                                resNode.TryGetValue("maxAmount", ref maxAmount);
                                var definition = PartResourceLibrary.Instance.GetDefinition(name);
                                var density = definition.density;
                                float resMass = maxAmount * density;
                                totalResMass += resMass;
                            }

                            if (totalResMass > mass)
                                continue;
                        }
                        stringBuilder = new StringBuilder();

                        Bounds bounds = default(Bounds);
                        foreach (Bounds rendererBound in PartGeometryUtil.GetRendererBounds((Part)current.partPrefab))
                            bounds.Encapsulate(rendererBound);

                        if (!isCargoPart)
                        {
                            float vol = (float)(bounds.size.x * bounds.size.y * bounds.size.z) * 1000f;

                            if (vol > Settings.largestAllowablePart && Settings.limitSize)
                                continue;

                            numCargoPartsAdded++;


                            var adjVol = AdjustedVolume(current, vol, isEnginePart, isRcsPart, out float adj);

                            if (currentCargoPart != null)
                            {
                                currentCargoPart.SetValue("packedVolume", adjVol.ToString("F0"));
                            }


                            stringBuilder.AppendLine("// " + current.partUrl);
                            stringBuilder.AppendLine(string.Format("// Bounding Box Size: {0} liters", vol));
                            stringBuilder.AppendLine("// Volume adjustment: " + (adj * 100).ToString("F0") + "%");
                            if (isRcsPart)
                                stringBuilder.AppendLine("// RCS module detected");
                            if (isEnginePart)
                                stringBuilder.AppendLine("// Engine module detected");
                            stringBuilder.AppendLine("//");
                            string partName = urlParts[urlParts.Length - 1];
                            
                            stringBuilder.AppendLine("@PART[" + partName.Replace(' ', '?') + "]:HAS[!MODULE[ModuleCargoPart]]:Final");
                            stringBuilder.AppendLine("{");
                            stringBuilder.AppendLine("    MODULE");
                            stringBuilder.AppendLine("    {");
                            stringBuilder.AppendLine("        name = ModuleCargoPart");
                            stringBuilder.AppendLine("        packedVolume = " + adjVol.ToString("F0"));
                            stringBuilder.AppendLine("    }");
                            stringBuilder.AppendLine("}");
                            stringBuilder.AppendLine("// ----------------------------------------------------------------------");

                        }
                        modifiedParts.Add(current.partUrl, stringBuilder);
                    }
                }
            }

            stringBuilder = new StringBuilder();

            foreach (var d in modifiedParts)
                stringBuilder.Append(d.Value.ToString());

            File.AppendAllText(VOL_CFG_FILE, stringBuilder.ToString());

            stopwatch.Stop();
            Log.Info("File written to " + VOL_CFG_FILE);
            Log.Info(string.Format("Run in {0}ms", (object)stopwatch.ElapsedMilliseconds));
            if (numCargoPartsAdded > 0)
                ShowWarning(numCargoPartsAdded);
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
            return (float)(vol * (1.0 + num) + 1.0);
        }
    }
}
