using UnityEngine;
using ToolbarControl_NS;


namespace KSP_PartVolume
{

    public partial class PartVolume : MonoBehaviour
    {
        static internal ToolbarControl toolbarControl = null;

        Rect RectGUI
        {
            get
            {
                return new Rect(0, Screen.height - 50, Screen.width, 100);
            }
        }

        Texture2D btn = null;
        GUIContent guiContent;
        void OnGUI2()
        {
            if (btn == null)
            {
                btn = new Texture2D(2, 2);
                guiContent = new GUIContent();
                guiContent.text = "  KSP PartVolume Settings";
                if (!ToolbarControl.LoadImageFromFile(ref btn, "GameData/KSP_PartVolume/PluginData/PartVolume-38"))
                    Log.Error("Unable to load image from file");
                else
                    guiContent.image = btn;
            }
            GUI.skin = HighLogic.Skin;
            GUILayout.BeginArea(RectGUI);
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(guiContent, GUILayout.Width(200)))
                ToggleWin();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        void ToggleWin()
        {
            visible = !visible;
            if (visible)
            {
                Settings.RememberSettings();
                partVolSettingsRect.x = (Screen.width - partVolSettingsRect.width) / 2;
                partVolSettingsRect.y = (Screen.height - partVolSettingsRect.height) / 2;
            }
            else
            {
                Settings.SaveConfig();
            }
        }


    }
}
