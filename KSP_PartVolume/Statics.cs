﻿using System.Collections.Generic;
using static KSP_PartVolume.PartVolume;
using KSP.Localization;

namespace KSP_PartVolume
{
    internal static class Statics
    {
        internal static SortedDictionary<string, PartModification> modifiedParts = new SortedDictionary<string, PartModification>();

        internal static void Check4DelModCargoPart(Part part)
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
                        if (modifiedParts[part.partInfo.partUrl].delModuleCargoPart || modifiedParts[part.partInfo.partUrl].packedVolume == -999)
                            modToDel = module;
                    }
                    break;
                }
            }
            if (modToDel != null)
            {
                part.RemoveModule(modToDel);
                Log.Info("Deleting ModuleCargoPart from part: " + part.name);
            }
        }

    }
}
