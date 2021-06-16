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
            if (HighLogic.LoadedSceneIsFlight && Statics.modifiedParts.ContainsKey(this.part.partInfo.partUrl))
            {
                Log.Info("KSPPartVolumeModule.Start");
                Statics.Check4DelModCargoPart(part);

#if false
                ProtoPartSnapshot pps = part.protoPartSnapshot;
                foreach (var m in pps.modules)
                {
                    if (m.moduleName == "ModuleCargoPart")
                    {
                        Log.Info("ModuleCargoPart found");
                        var currentCargoPart = m.moduleValues;
                        Log.Info("ModuleCargoPart.configs: " + currentCargoPart);
                        if (currentCargoPart.HasValue("packedVolume"))
                        {
                            var s = currentCargoPart.GetValue("packedVolume");
                            Log.Info("packedVolume found: " + s);
                            currentCargoPart.SetValue("packedVolume", packedVolume.ToString("F0"));
                        }
                        else
                            Log.Error("packedVolume not found");
                    }
                }
#endif

#if false
                var partConfig = part.partInfo.partConfig;
                if (partConfig != null)
                {
                    Log.Info("partconfig found");
                    var moduleNodes = partConfig.GetNodes("MODULE");
                    Log.Info("partConfig: " + moduleNodes);
                    Log.Info("moduleNodes.Count: " + moduleNodes.Length);
                    for (int i = 0; i < moduleNodes.Length; i++)
                    {
                        Log.Info("name: " + moduleNodes[i].GetValue("name"));
                        if (moduleNodes[i].GetValue("name") == "ModuleCargoPart")
                        {
                            Log.Info("ModuleCargoPart found");
                            var currentCargoPart = moduleNodes[i];

                            if (currentCargoPart.HasValue("packedVolume"))
                            {
                                var s = currentCargoPart.GetValue("packedVolume");
                                Log.Info("packedVolume found: " + s);
                                currentCargoPart.SetValue("packedVolume", packedVolume.ToString("F0"));
                            }
                            else
                                Log.Error("packedVolume not found");
                        }
                    }
                }
#endif
            }
        }

#if false
        public string GetInfo()
        {
            return "PartVolumeModule, packedVolume: " + packedVolume.ToString("F3");
        }
#endif
    }
}
