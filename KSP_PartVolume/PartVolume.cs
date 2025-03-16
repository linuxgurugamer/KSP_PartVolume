using KSP.Localization;
using KSP_Log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

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

    [KSPAddon(KSPAddon.Startup.Instantly, false)]
    public class CheckOldFile : MonoBehaviour
    {
        public static bool oldFileDeleted = false;
        public void Awake()
        {
            for (int i = 0; i < PartVolume.FILE_VERSION; i++)
            {
                if (File.Exists(PartVolume.FileName(i)))
                {
                    File.Delete(PartVolume.FileName(i));
                    oldFileDeleted = true;
                }
            }

        }
    }

    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
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
        internal const string PARTWHITELIST = "PARTVOLUME_WHITELIST";

        bool visible = false;
        static bool RestartWindowVisible = false;
        static bool newPartsDetected = false;
        List<string> resourceBlackList;
        List<String> partBlacklist;
        List<String> modBlacklist;
        List<String> moduleBlacklist;
        List<String> partWhitelist;
        string blacklistRegexPattern = "";
        //string WhitelistRegexPattern = "";

        internal const string FILENAME_PREFIX = "partVolumes";
        //
        // Increment when old file needs to be invalidated and deleted automatically
        //
        internal const int FILE_VERSION = 5;

        internal static string FileName(int i)
        {
            if (i > 0)
                return KSPUtil.ApplicationRootPath + "GameData/" + FILENAME_PREFIX + "-v" + i.ToString() + ".cfg";
            return KSPUtil.ApplicationRootPath + "GameData/" + FILENAME_PREFIX + ".cfg";
        }

        private void Awake()
        {
            Instance = this;
#if DEBUG
            Log = new Log("KSP_PartVolume", Log.LEVEL.INFO);
#else
            Log = new Log("KSP_PartVolume", Log.LEVEL.ERROR);
#endif

            VOL_CFG_FILE = FileName(FILE_VERSION);

            CFG_FILE = KSPUtil.ApplicationRootPath + "GameData/" + MODDIR + "/PluginData/KSP_PartVolume.cfg";
            KIFA = KSPUtil.ApplicationRootPath + "GameData/KerbalInventoryForAll/AllowModPartsInStock.cfg";
            RES_BLACKLIST = KSPUtil.ApplicationRootPath + "GameData/" + MODDIR + "/PluginData/ResourceBlacklist.txt";

            var blacklistFile = File.ReadAllLines(RES_BLACKLIST);
            resourceBlackList = new List<string>(blacklistFile);
            partBlacklist = new List<string>();
            modBlacklist = new List<string>();
            moduleBlacklist = new List<string>();
            partWhitelist = new List<String>();

            ConfigNode[] partBlacklistNodes = GameDatabase.Instance.GetConfigNodes(PARTBLACKLIST);
            for (int i = 0; i < partBlacklistNodes.Length; i++)
            {
                var n = partBlacklistNodes[i];
                var v = n.GetValues("blacklistPart");
                foreach (var v1 in v)
                    partBlacklist.Add(v1);
                v = n.GetValues("blacklistModDir");
                foreach (var v2 in v)
                    modBlacklist.Add(v2);

                v = n.GetValues("blacklistModule");
                foreach (var v3 in v)
                    moduleBlacklist.Add(v3);


                var patterns = n.GetValues("blacklistRegexPattern");
                blacklistRegexPattern = String.Join("|", patterns.Select(x => "(" + x + ")"));
            }
            //
            // Note:  Due to the time this runs, log lines will NOT be written to the normal
            // KSP log.  However, they WILL be written to the Logs/SpaceTux/KSP_PartVolume.log file
            //
            foreach (var p in partBlacklist)
            {
                Log.Info("Part blacklisted: " + p);
            }
            foreach (var p in moduleBlacklist)
            {
                Log.Info("Module blacklisted: " + p);
            }
            foreach (var p in modBlacklist)
            {
                Log.Info("Mod directory blacklisted: " + p);
            }

            ConfigNode[] partWhitelistNodes = GameDatabase.Instance.GetConfigNodes(PARTWHITELIST);
            foreach (var n in partWhitelistNodes)
            {
                var v = n.GetValues("whitelistPart");
                foreach (var v1 in v)
                    partWhitelist.Add(v1);

                //var patterns = n.GetValues("whitelistRegexPattern");
                //WhitelistRegexPattern = String.Join("|", patterns.Select(x => "(" + x + ")"));
            }
            foreach (var p in partWhitelist)
            {
                Log.Info("Part whitelisted: " + p);
            }
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

                    string[] urlParts = current.partUrl.Split('/');
                    bool blacklistedMod = false;
                    for (int ui = 0; ui < urlParts.Length - 1; ui++)
                    {
                        if (modBlacklist.Contains(urlParts[ui]))
                        {
                            Log.Info(String.Format("ModDirName: {0, -40} found in the blacklist and ignored.", urlParts[ui] + ","));
                            blacklistedMod = true;
                            break;
                        }
                    }
                    if (blacklistedMod) { continue; }
                    string urlName = urlParts[urlParts.Length - 1];

                    //
                    // urlName more precisely correspond to part name in the config than current.name,
                    // but KerbalEVA and flag have empty urlname and need to be filtered, so: 
                    //

                    string partName = urlParts[urlParts.Length - 1];
                    if (partName == "") partName = current.name;

                    if (partBlacklist.Contains(partName) ||
                        Regex.IsMatch(partName, blacklistRegexPattern))
                    {
                        Log.Info(String.Format("partName: {0, -40} found in the blacklist and ignored.", partName + ","));
                        continue;
                    }

                    bool contains_ModuleCargoPart = false;
                    bool contains_ModuleInventoryPart = false;
                    bool contains_KSPPartVolumeModule = false;
                    bool contains_ModuleGroundPart = false;

                    bool containsCrew = false;
                    bool isTank = false;
                    bool isTankNotIgnoreable = false;
                    bool sizeTooBig = false;
                    bool isStock = false;

                    bool isRcsPart = false;
                    bool isEnginePart = false;
                    ConfigNode currentCargoPart = null;



                    if (!Settings.doStock)
                    {
                        if (urlParts[0] == "Squad" || urlParts[0] == "SquadExpansion")
                            if (!partWhitelist.Contains(partName))
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
                        for (int j = 0; j < moduleBlacklist.Count; j++)
                        {
                            if (name == moduleBlacklist[j])
                            {
                                contains_ModuleCargoPart = true;
                                break;
                            }
                        }
                        //if (name == "ModuleGroundPart")
                        //contains_ModuleGroundPart = true;
                        if (name == "ModuleInventoryPart")
                            contains_ModuleInventoryPart = true;
                        if (name == "KSPPartVolumeModule")
                            contains_KSPPartVolumeModule = true;
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

                    //if (!Settings.doTanks)
                    {
                        for (int i = 0;i < resNodes.Length;i++) 
                        {
                            var name = resNodes[i].GetValue("name");

                            if (resourceBlackList.Contains(name))
                                continue;

                            float maxAmount = 0;
                            resNodes[i].TryGetValue("maxAmount", ref maxAmount);
                            var definition = PartResourceLibrary.Instance.GetDefinition(name);
                            if (definition != null)
                            {
                                var density = definition.density;
                                float resMass = maxAmount * density;
                                totalResMass += resMass;
                            }
                        }

                        if (totalResMass > mass)
                        {
                            isTankNotIgnoreable = true;
                            isTank = !Settings.doTanks;
                        }
                    }
                    stringBuilder = new StringBuilder();

                    Bounds bounds = default(Bounds);

                    foreach (Bounds rendererBound in PartGeometryUtil.GetRendererBounds((Part)current.partPrefab))
                        bounds.Encapsulate(rendererBound);

#if false
                        Bounds colliderBounds = default(Bounds);
                        foreach (Bounds rendererBound in PartGeometryUtil.GetPartColliderBounds((Part)current.partPrefab))
                            colliderBounds.Encapsulate(rendererBound);

                        Bounds allBounds = default(Bounds);
                        var a = PartGeometryUtil.GetPartColliderBounds(current.partPrefab);
                        allBounds = PartGeometryUtil.MergeBounds(a, current.iconPrefab.transform.root);
#endif

                    float vol = (float)(bounds.size.x * bounds.size.y * bounds.size.z) * 1000f;

                    if (vol > Settings.largestAllowablePart && Settings.limitSize)
                        sizeTooBig = true;

                    var maxLen = Math.Max(bounds.size.x, Math.Max(bounds.size.y, bounds.size.z));
                    var minLen = Math.Min(bounds.size.x, Math.Min(bounds.size.y, bounds.size.z));
                    var tankVol = Math.Pow(minLen * 0.5, 2) * maxLen * Math.PI * 1000;

                    var adjVol = AdjustedVolume(current, vol, isEnginePart, isRcsPart, out float adj);

                    int stackableQuantity = 1;
                    if ((int)adjVol != 0)
                        stackableQuantity = Math.Min((int)Settings.maxCommonStackVolume / (int)adjVol, Settings.maxPartsInStack);

                    bool isManipulableOnly = false;
                    bool isKSP_PartVolumeModule = false;
                    string currentCargoPartPackedVolume = "";

                    if (currentCargoPart != null)
                    {
                        Log.Info("currentCargoPart: " + current.name);
                        if (currentCargoPart.HasValue("packedVolume"))
                        {
                            currentCargoPartPackedVolume = currentCargoPart.GetValue("packedVolume");
                            currentCargoPart.SetValue("packedVolume", adjVol.ToString("F0"));

                            Log.Info(String.Format("partName: {0, -40} packedVolume: {1,7}, calcPackedVolume: {2,7:F0}",
                                partName + ",", currentCargoPartPackedVolume, adjVol));

                            var v = float.Parse(currentCargoPartPackedVolume);
                            if (v <= 0)
                                isManipulableOnly = true;

                            isKSP_PartVolumeModule = currentCargoPart.HasValue("KSP_PartVolume");
                        }
                        else
                        {
                            Log.Error(String.Format("partName: {0, -40} packedVolume not found", partName + ","));
                        }
                    }
                    if (contains_ModuleInventoryPart)
                        adjVol = -1;

                    StringBuilder tmp = new StringBuilder();
                    tmp.AppendLine("// " + current.partUrl);

                    tmp.AppendLine("// Dimensions: x: " + bounds.size.x.ToString("F2") + ", y: " + bounds.size.y.ToString("F2") + ", z: " + bounds.size.z.ToString("F2"));

                    tmp.AppendLine(string.Format("// Bounding Box Size: {0} liters", vol));
                    tmp.AppendLine("// Volume adjustment: " + (adj * 100).ToString("F0") + "%");
                    if (isRcsPart)
                        tmp.AppendLine("// RCS module detected");
                    if (isEnginePart)
                        tmp.AppendLine("// Engine module detected");
                    Part part = UnityEngine.Object.Instantiate(current.partPrefab);
                    part.gameObject.SetActive(value: false);


                    if (!containsCrew && !isTank && !sizeTooBig && !isStock && !contains_ModuleInventoryPart && !contains_ModuleGroundPart &&
                        (!contains_ModuleCargoPart ||
                         (contains_ModuleCargoPart && Settings.processManipulableOnly && isManipulableOnly) ||
                       (!isKSP_PartVolumeModule && partWhitelist.Contains(partName))
                        ))
                    {
                        if (isTankNotIgnoreable)
                        {
                            var volume = DetermineVolume(part) * 1000;
                            stringBuilder.AppendLine("//      Calculated tank volume: " + volume.ToString("F1"));
                            stringBuilder.AppendLine("//      Calculated tankVol (max x min) volume: " + tankVol.ToString("F1"));
                        }

                        tmp.AppendLine("//");


                        stringBuilder.Append(tmp);
                        string adjName = partName.Replace(' ', '?').Replace('(', '?').Replace(')', '?');
                        if (contains_ModuleCargoPart)
                        {
                            stringBuilder.AppendLine("@PART[" + adjName + "]:HAS[@MODULE[ModuleCargoPart]]:Final");
                            stringBuilder.AppendLine("{");
                            stringBuilder.AppendLine("    @MODULE[ModuleCargoPart]");
                            stringBuilder.AppendLine("    {");
                            stringBuilder.AppendLine("        %packedVolume = " + adjVol.ToString("F0"));
                        }
                        else
                        {
                            stringBuilder.AppendLine("@PART[" + adjName + "]:HAS[!MODULE[ModuleCargoPart]]:Final");
                            stringBuilder.AppendLine("{");
                            stringBuilder.AppendLine("    MODULE");
                            stringBuilder.AppendLine("    {");
                            stringBuilder.AppendLine("        name = ModuleCargoPart");
                            stringBuilder.AppendLine("        %packedVolume = " + adjVol.ToString("F0"));
                        }


                        if (Settings.stackParts && stackableQuantity > 1)
                        {
                            stringBuilder.AppendLine("        %stackableQuantity = " + stackableQuantity);
                        }

                        stringBuilder.AppendLine("        %KSP_PartVolume = true");
                        stringBuilder.AppendLine("    }");
                        stringBuilder.AppendLine("}");

                        RestartWindowVisible = true;
                        newPartsDetected = true;
                        part = UnityEngine.Object.Instantiate(current.partPrefab);
                        part.gameObject.SetActive(value: false);
                        for (int j = 0; j < part.Modules.Count; j++)
                        { 
                            if (part.Modules[j].moduleName == "ModuleCargoPart")
                            {
                                var mcp = part.Modules[j] as ModuleCargoPart;
                                mcp.part = part;
                                mcp.packedVolume = adjVol;

                                if (stackableQuantity > 1)
                                    mcp.stackableQuantity = stackableQuantity;

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
                            if (isStock)
                                stringBuilder.AppendLine("//      is stock");
                            if (contains_ModuleCargoPart && !Settings.processManipulableOnly
                                || contains_ModuleCargoPart && !isManipulableOnly)
                                stringBuilder.AppendLine("//      contains ModuleCargoPart (packedVolume = " + currentCargoPartPackedVolume + ")");
                            if (contains_ModuleInventoryPart)
                                stringBuilder.AppendLine("//      contains ModuleInventoryPart");
                            if (contains_ModuleGroundPart)
                                stringBuilder.AppendLine("//      contains ModuleGroundPart");
                            stringBuilder.AppendLine("//");

                            adjVol = -999;
#if true
                            current.partConfig.RemoveNode(currentCargoPart);
                            //Part part = UnityEngine.Object.Instantiate(current.partPrefab);
                            //part.gameObject.SetActive(value: false);

                            Statics.Check4DelModCargoPart(part);
                            //Destroy(part);
#endif
                        }
                    }
                    Destroy(part);
                    if (!Statics.modifiedParts.ContainsKey(current.partUrl))
                    {
                        Statics.modifiedParts.Add(current.partUrl, new PartModification(stringBuilder, adjVol, adjVol == -999));
                        Log.Info("Modified part: " + current.partUrl);
                    }
                    else
                        Log.Error("modifiedParts already contains: " + current.partUrl);
                    if (!fileExists)
                        stringBuilder.AppendLine("// ----------------------------------------------------------------------");

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


        // From @Taniwaha
        double DetermineVolume(Part part)
        {
            var mfList = part.FindModelComponents<MeshFilter>();
            double vol = 0;
            for (int j = 0; j < mfList.Count;j++)
            { 
                Log.Info("Part: " + part.partName + ", mesh: " + mfList[j].name);
                Mesh mesh = mfList[j].sharedMesh;
                Vector3[] verts = mesh.vertices;
                for (int sm = 0; sm < mesh.subMeshCount; sm++)
                {
                    int[] tris = mesh.GetTriangles(sm);
                    for (int i = 0; i < tris.Length; i += 3)
                    {
                        Vector3d a = verts[tris[i]];
                        Vector3d b = verts[tris[i + 1]];
                        Vector3d c = verts[tris[i + 2]];
                        vol += Vector3d.Dot(c, Vector3d.Cross(a, b)) / 6;
                    }
                }
            }
            return vol;
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
            if (Settings.enableFillers)
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
            else
            {
                adj = 0;
                return vol;
            }
        }
    }
}
