
using UnityEngine;
using ToolbarControl_NS;

namespace KSP_PartVolume
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class RegisterToolbar : MonoBehaviour
    {

#if false
         internal static Log Log = null;
       void Awake()
        {
            if (Log == null)
#if DEBUG
                Log = new Log("LogNotes", Log.LEVEL.INFO);
#else
          Log = new Log("LogNotes", Log.LEVEL.ERROR);
#endif

        }
#endif

        void Awake()
        {
            ToolbarControl.RegisterMod(PartVolume.MODID, PartVolume.MODNAME);
        }

    }
}
