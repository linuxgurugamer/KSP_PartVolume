using SpaceTuxUtility;
using System.IO;
using static KSP_PartVolume.PartVolume;

namespace KSP_PartVolume
{
    class Settings
    {

        public static bool enableFillers = true;
        public static float filler = 0.1f;
        public static float scienceFiller = 0.25f;
        public static float engineFiller = 0.15f;
        public static float rcsFiller = 0.2f;
        public static bool doTanks = false;
        public static bool limitSize = true;
        public static float largestAllowablePart = 64000f;
        public static bool manned = false;
        public static bool doStock = false;
        public static bool processManipulableOnly = false;
        public static bool stackParts = false;
        public static int maxPartsInStack = 4;
        public static float maxCommonStackVolume = 60f;
        public static bool hideUnlessChangesDetected = false;

        public static bool oEnableFillers = enableFillers;
        public static float oFiller = filler;
        public static float oScienceFiller = scienceFiller;
        public static float oEngineFiller = engineFiller;
        public static float oRcsFiller = rcsFiller;
        public static bool oDotanks = doTanks;
        public static bool oLimitSize = limitSize;
        public static float oLargestAllowablePart = largestAllowablePart;
        public static bool oManned = manned;
        public static bool oDoStock = doStock;
        public static bool oProcessManipulableOnly = processManipulableOnly;
        public static bool oStackParts = stackParts;
        public static int oMaxPartsInStack = maxPartsInStack;
        public static float oMaxStackCommonVolume = maxCommonStackVolume;
        public static bool oHideUnlessChangesDetected = hideUnlessChangesDetected;

        static internal void ResetToDefaults()
        {
            enableFillers = true;
            filler = 0.1f;
            scienceFiller = 0.25f;
            engineFiller = 0.15f;
            rcsFiller = 0.2f;
            doTanks = false;
            limitSize = true;
            largestAllowablePart = 64000f;
            manned = false;
            doStock = false;
            processManipulableOnly = false;
            stackParts = false;
            maxPartsInStack = 4;
            maxCommonStackVolume = 60f;
            hideUnlessChangesDetected = false;
    }
        static internal void RememberSettings()
        {
            oEnableFillers = enableFillers;
            oFiller = filler;
            oEngineFiller = engineFiller;
            oRcsFiller = rcsFiller;
            oScienceFiller = scienceFiller;
            oDotanks = doTanks;
            oLimitSize = limitSize;
            oLargestAllowablePart = largestAllowablePart;
            oManned = manned;
            oDoStock = doStock;
            oProcessManipulableOnly = processManipulableOnly;
            oStackParts = stackParts;
            oMaxPartsInStack = maxPartsInStack;
            oMaxStackCommonVolume = maxCommonStackVolume;
            oHideUnlessChangesDetected = hideUnlessChangesDetected;
        }
        static internal void LoadConfig()
        {
            ConfigNode configNode = ConfigNode.Load(CFG_FILE);
            if (configNode == null)
                return;
            ConfigNode node = configNode.GetNode("PARTVOLUME");
            if (node == null)
                return;


            enableFillers = node.SafeLoad("enableFillers", enableFillers);
            filler = node.SafeLoad("filler", filler);
            scienceFiller = node.SafeLoad("scienceFiller", scienceFiller);
            engineFiller = node.SafeLoad("engineFiller", engineFiller);
            rcsFiller = node.SafeLoad("rcsFiller", rcsFiller);
            doTanks = node.SafeLoad("doTanks", doTanks);
            limitSize = node.SafeLoad("limitSize", limitSize);
            largestAllowablePart = node.SafeLoad("largestAllowablePart", largestAllowablePart);
            manned = node.SafeLoad("manned", manned);
            doStock = node.SafeLoad("doStock", doStock);
            processManipulableOnly = node.SafeLoad("processManipulableOnly", processManipulableOnly);
            stackParts = node.SafeLoad("stackParts", stackParts);
            maxPartsInStack = node.SafeLoad("maxPartsInStack", maxPartsInStack);
            maxCommonStackVolume = node.SafeLoad("maxStackCommonVolume", maxCommonStackVolume);
            hideUnlessChangesDetected = node.SafeLoad("hideUnlessChangesDetected", hideUnlessChangesDetected);
        }

        static internal void SaveConfig()
        {
            ConfigNode configNode1 = new ConfigNode();
            ConfigNode configNode2 = new ConfigNode("PARTVOLUME");
            configNode2.AddValue("enableFillers", enableFillers);
            configNode2.AddValue("filler", filler);
            configNode2.AddValue("scienceFiller", scienceFiller.ToString("F2"));
            configNode2.AddValue("engineFiller", engineFiller.ToString("F2"));
            configNode2.AddValue("rcsFiller", rcsFiller.ToString("F2"));
            configNode2.AddValue("doTanks", doTanks);
            configNode2.AddValue("limitSize", limitSize);
            configNode2.AddValue("largestAllowablePart", largestAllowablePart.ToString("F0"));
            configNode2.AddValue("manned", manned);
            configNode2.AddValue("doStock", doStock);
            configNode2.AddValue("processManipulableOnly", processManipulableOnly);
            configNode2.AddValue("stackParts", stackParts);
            configNode2.AddValue("maxPartsInStack", maxPartsInStack);
            configNode2.AddValue("maxStackCommonVolume", maxCommonStackVolume);
            configNode2.AddValue("hideUnlessChangesDetected", hideUnlessChangesDetected);

            configNode1.AddNode(configNode2);
            configNode1.Save(PartVolume.CFG_FILE);

            if (oEnableFillers != enableFillers ||
                oEngineFiller != engineFiller ||
                oFiller != filler ||
                oRcsFiller != rcsFiller ||
                oScienceFiller != scienceFiller ||
                oDotanks != doTanks ||
                oLimitSize != limitSize ||
                oLargestAllowablePart != largestAllowablePart ||
                oManned != manned ||
                oDoStock != doStock ||
                oProcessManipulableOnly != processManipulableOnly ||
                oStackParts != stackParts ||
                oMaxPartsInStack != maxPartsInStack ||
                oMaxStackCommonVolume != maxCommonStackVolume ||
                oHideUnlessChangesDetected != hideUnlessChangesDetected
                )
            {
                File.Delete(PartVolume.VOL_CFG_FILE);
                PartVolume.Instance.ShowWarning();
            }
        }

    }
}
