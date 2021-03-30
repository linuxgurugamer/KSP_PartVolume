using SpaceTuxUtility;
using System.IO;
using static KSP_PartVolume.PartVolume;

namespace KSP_PartVolume
{
    class Settings
    {
        public static float filler = 0.1F;
        public static float scienceFiller = 0.25F;
        public static float engineFiller = 0.15F;
        public static float rcsFiller = 0.2F;
        public static bool doTanks = false;
        public static bool limitSize = true;
        public static float largestAllowablePart = 64000f;

        public static float oFiller = 0.1F;
        public static float oScienceFiller = 0.25F;
        public static float oEngineFiller = 0.15F;
        public static float oRcsFiller = 0.2F;
        public static bool oDotanks = false;
        public static bool oLimitSize = true;
        public static float oLargestAllowablePart = 64000f;

        static internal void RememberSettings()
        {
            oFiller = filler;
            oEngineFiller = engineFiller;
            oRcsFiller = rcsFiller;
            oScienceFiller = scienceFiller;
            oDotanks = doTanks;
            oLimitSize = limitSize;
            oLargestAllowablePart = largestAllowablePart;

        }
        static internal void LoadConfig()
        {
            Log.Info(nameof(LoadConfig));
            ConfigNode configNode = ConfigNode.Load(PartVolume.CFG_FILE);
            Log.Info("Volume.CFG_FILE: " + PartVolume.CFG_FILE);
            if (configNode == null)
                return;
            ConfigNode node = configNode.GetNode("PARTVOLUME");
            if (node == null)
                return;
            filler = node.SafeLoad("filler", filler);
            scienceFiller = node.SafeLoad("scienceFiller", scienceFiller);
            engineFiller = node.SafeLoad("engineFiller", engineFiller);
            rcsFiller = node.SafeLoad("rcsFiller", rcsFiller);
            doTanks = node.SafeLoad("doTanks", doTanks);
            limitSize = node.SafeLoad("limitSize", limitSize);
            largestAllowablePart = node.SafeLoad("largestAllowablePart", largestAllowablePart);
        }

        static internal void SaveConfig()
        {
            Log.Info(nameof(SaveConfig));
            ConfigNode configNode1 = new ConfigNode();
            ConfigNode configNode2 = new ConfigNode("PARTVOLUME");
            configNode2.AddValue("filler", filler);
            configNode2.AddValue("scienceFiller", scienceFiller);
            configNode2.AddValue("engineFiller", engineFiller);
            configNode2.AddValue("rcsFiller", rcsFiller);
            configNode2.AddValue("dotanks", doTanks);
            configNode2.AddValue("limitSize", limitSize);
            configNode2.AddValue("largestAllowablePart", largestAllowablePart);

            configNode1.AddNode(configNode2);
            configNode1.Save(PartVolume.CFG_FILE);

            if (oEngineFiller != engineFiller || 
                oFiller != filler ||
                oRcsFiller != rcsFiller ||
                oScienceFiller != scienceFiller ||
                oDotanks != doTanks ||
                oLimitSize != limitSize ||
                oLargestAllowablePart != largestAllowablePart)
            {
                File.Delete(PartVolume.VOL_CFG_FILE);
                PartVolume.Instance.ShowWarning();
            }
        }

    }
}
