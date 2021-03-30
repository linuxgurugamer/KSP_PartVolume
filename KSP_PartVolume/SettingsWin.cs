
using System;
using SpaceTuxUtility;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEngine;
using KSP_Log;
using KSP.UI.Screens;

using ToolbarControl_NS;
using ClickThroughFix;


namespace KSP_PartVolume
{

    public partial class PartVolume : MonoBehaviour
    {
        int winId = SpaceTuxUtility.WindowHelper.NextWindowId("PartVolSettings");
        Rect toolbarRect = new Rect(0, 0, 400, 150);

        void OnGUI()
        {
            if (visible) 
            {
                GUI.skin = HighLogic.Skin;
                toolbarRect = ClickThruBlocker.GUILayoutWindow(winId, toolbarRect, ToolbarWindow, "Part Volume Settings");
            }
        }

        void ToolbarWindow(int windowID)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Filler (" + (Settings.filler * 100).ToString("F0") + "%):");
            GUILayout.FlexibleSpace();
            Settings.filler = GUILayout.HorizontalSlider(Settings.filler, 0, 1, GUILayout.Width(250));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Science Filler (" + (Settings.scienceFiller * 100).ToString("F0") + "%):");
            GUILayout.FlexibleSpace();
            Settings.scienceFiller = GUILayout.HorizontalSlider(Settings.scienceFiller, 0, 1, GUILayout.Width(250));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Engine Filler (" + (Settings.engineFiller * 100).ToString("F0") + "%):");
            GUILayout.FlexibleSpace();
            Settings.engineFiller = GUILayout.HorizontalSlider(Settings.engineFiller, 0, 1, GUILayout.Width(250));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("RCS Filler (" + (Settings.rcsFiller * 100).ToString("F0") + "%):");
            GUILayout.FlexibleSpace();
            Settings.rcsFiller = GUILayout.HorizontalSlider(Settings.rcsFiller, 0, 1, GUILayout.Width(250));
            GUILayout.EndHorizontal();
            Settings.doTanks = GUILayout.Toggle(Settings.doTanks, "Include tanks");

            GUILayout.BeginHorizontal();
            Settings.limitSize = GUILayout.Toggle(Settings.limitSize, "Limit Size");
            GUILayout.EndHorizontal();
            if (Settings.limitSize)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Largest Allowable Part: ");
                var s = GUILayout.TextField(Settings.largestAllowablePart.ToString("F0"), GUILayout.Width(50));
                if (float.TryParse(s, out float f))
                {
                    Settings.largestAllowablePart = f;
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.Space(20);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Save & Close", GUILayout.Width(90)))
            {
                visible = false;
                Settings.SaveConfig();
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Cancel", GUILayout.Width(90)))
            {
                visible = false;
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUI.DragWindow();

        }
    }
}