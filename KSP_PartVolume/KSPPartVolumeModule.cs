using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

using static KSP_PartVolume.PartVolume;

namespace KSP_PartVolume
{
    public class KSPPartVolumeModule : PartModule
    {
        [KSPField(guiName = "Packed Volume", guiActiveEditor = true, guiActive = true)]
        public float packedVolume = 0;

        public void Start()
        {
            Log.Info("KSPPartVolumeModule.Start, Part: " + this.part.name);
            if (Statics.modifiedParts.ContainsKey(this.part.partInfo.partUrl))
            {
                Log.Info("Part: " + this.part.name + ", packedVolume: " + Statics.modifiedParts[part.partInfo.partUrl].packedVolume);
                Statics.DelModCargoPart(part);
            }
            foreach (PartModule m in part.Modules)
            {
                if (m.moduleName == "ModuleCargoPart")
                {
                    var mcp = m as ModuleCargoPart;
                    this.packedVolume = mcp.packedVolume;
                }
            }
        }
    }
}
