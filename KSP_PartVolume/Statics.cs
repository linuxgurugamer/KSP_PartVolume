using System.Collections.Generic;
using static KSP_PartVolume.PartVolume;
using KSP.Localization;

namespace KSP_PartVolume
{
    internal static class Statics
    {
        internal static SortedDictionary<string, PartModification> modifiedParts = new SortedDictionary<string, PartModification>();

        internal static void DelModCargoPart(Part part)
        {
            PartModule modToDel = null;
            foreach (PartModule module in part.Modules)
            {
                if (module.moduleName == "ModuleCargoPart")
                {
                    var mcp = module as ModuleCargoPart;
                    if (modifiedParts.ContainsKey(part.partInfo.partUrl))
                    {
                        mcp.packedVolume = modifiedParts[part.partInfo.partUrl].packedVolume;
                        Log.Info("packedVolume: " + mcp.packedVolume);
                        if (modifiedParts[part.partInfo.partUrl].delModuleCargoPart || modifiedParts[part.partInfo.partUrl].packedVolume == -999)
                            modToDel = module;
                    }
                    else
                    {
                        //modToDel = module;
                    }
                    break;
                }
            }
            if (modToDel != null)
            {
                part.RemoveModule(modToDel);
                Log.Info("Deleting ModuleCargoPart from part: " + part.name);

                AvailablePart.ModuleInfo mitodel = null;
                for (int i = part.partInfo.moduleInfos.Count - 1; i >= 0; --i)
                {
                    AvailablePart.ModuleInfo info = part.partInfo.moduleInfos[i];
                    if (info.moduleName == Localizer.Format("#autoLOC_8002221")) // Cargo Part
                    {
                        mitodel = info;
                        break;
                    }
                }
                if (mitodel != null)
                {
                    part.partInfo.moduleInfos.Remove(mitodel);
                }
            }
        }

    }
}
