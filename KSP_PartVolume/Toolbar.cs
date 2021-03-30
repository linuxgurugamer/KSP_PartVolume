using UnityEngine;
using KSP.UI.Screens;
using ToolbarControl_NS;


namespace KSP_PartVolume
{

    public partial class PartVolume : MonoBehaviour
    {
        static internal ToolbarControl toolbarControl = null;

        void AddToolbarButton()
        {
            {
                if (toolbarControl == null)
                {
                    toolbarControl = gameObject.AddComponent<ToolbarControl>();
                    toolbarControl.AddToAllToolbars(ToggleWin, ToggleWin,
                        ApplicationLauncher.AppScenes.MAINMENU,
                        MODID,
                        "partVolButton",
                        "KSP_PartVolume/PluginData/PartVolume-38",
                        "KSP_PartVolume/PluginData/PartVolume-24",
                        MODNAME
                    );

                }
            }
        }

        void ToggleWin()
        {
            visible = !visible;
            if (visible)
            {
                Settings.RememberSettings();
            }
            else
            {
                Settings.SaveConfig();
            }
        }


    }
}
