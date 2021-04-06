using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using KSP.Localization;
using static KSP_PartVolume.PartVolume;

namespace KSP_PartVolume
{
    public class KSPPartVolumeModule : PartModule
    {
        [KSPField(guiName = "Packed Volume", guiActiveEditor = true, guiActive = true)]
        public float packedVolume = 0;

        public void Start()
        {
            if (Statics.modifiedParts.ContainsKey(this.part.partInfo.partUrl))
            {
                if (HighLogic.LoadedSceneIsFlight)
                    Statics.Check4DelModCargoPart(part);
            }
        }
    }
}
