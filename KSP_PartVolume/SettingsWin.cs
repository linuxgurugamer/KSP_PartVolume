using System.IO;
using System.Collections.Generic;
using UnityEngine;
using ClickThroughFix;
using System.Diagnostics;

namespace KSP_PartVolume
{

    public partial class PartVolume : MonoBehaviour
    {
        int winId = SpaceTuxUtility.WindowHelper.NextWindowId("PartVolSettings");
        int restartWinId = SpaceTuxUtility.WindowHelper.NextWindowId("restartWinId");
        Rect partVolSettingsRect = new Rect(0, 0, 450, 150);
        Rect RestartWindowRect = new Rect(0, 0, 500, 200);

        void Start2()
        {
            RestartWindowRect.x = (Screen.width - RestartWindowRect.width) / 2;
            RestartWindowRect.y = (Screen.height - RestartWindowRect.height) / 2;
            GetCLIParams();
        }

        void OnGUI()
        {
            OnGUI2();
            if (visible)
            {
                GUI.skin = HighLogic.Skin;

                // Keep window on screen
                partVolSettingsRect.y = Mathf.Clamp(partVolSettingsRect.y, 0, Screen.height - partVolSettingsRect.height);
                partVolSettingsRect.x = Mathf.Clamp(partVolSettingsRect.x, 0, Screen.width - partVolSettingsRect.width);

                partVolSettingsRect = ClickThruBlocker.GUILayoutWindow(winId, partVolSettingsRect, ToolbarWindow, "Part Volume Settings");
            }
            if (RestartWindowVisible)
            {
                RestartWindowRect = ClickThruBlocker.GUILayoutWindow(restartWinId, RestartWindowRect, RestartWindow, "Restart Game");
            }
        }

        void RestartWindow(int winId)
        {
            GUILayout.BeginVertical();
            GUILayout.Label("New parts have been detected by KSP PartVolume");
            GUILayout.Label("The game will need to be restarted to have the changes properly implemented.");
            GUILayout.Label("Until that is done, the stock inventory system will not work properly with the newly-detected parts");
            GUILayout.Space(20);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Exit " + (Settings.restart?" & restart ":"") + "Game now!", GUILayout.Width(180)))
            {
                Log.Info("Trying Exit Game, application.Quit()");                
                OkToExit();
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Ignore and Continue", GUILayout.Width(180)))
            {
                Log.Info("Ignore and continue");
                RestartWindowVisible = false;
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUI.DragWindow();
        }
        string[] args = null;
        void GetCLIParams()
        {
            args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                Log.Info("Arg[" + i + "]: " + args[i]);
            }
        }

        void StartNewGame()
        {
            if (!Settings.restart)
                return;
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = args[0];
            startInfo.Arguments = "";
            for (int i = 1; i < args.Length; i++)
            {
                if (args[i].ToLower() != "-single-instance")
                    startInfo.Arguments += args[i];
                if (i < args.Length - 1)
                    startInfo.Arguments += " ";
            }

            Log.Info("startInfo.FileName: " + startInfo.FileName +
                ", startInfo.Arguments: " + startInfo.Arguments);
            Process.Start(startInfo);

        }

        void ToolbarWindow(int windowID)
        {
            Settings.enableFillers = GUILayout.Toggle(Settings.enableFillers, "Enable Fillers");

            if (Settings.enableFillers)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(50);
                GUILayout.Label("Filler (" + (Settings.filler * 100).ToString("F0") + "%):");
                GUILayout.FlexibleSpace();
                Settings.filler = GUILayout.HorizontalSlider(Settings.filler, 0, 1, GUILayout.Width(250));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(50);
                GUILayout.Label("Science Filler (" + (Settings.scienceFiller * 100).ToString("F0") + "%):");
                GUILayout.FlexibleSpace();
                Settings.scienceFiller = GUILayout.HorizontalSlider(Settings.scienceFiller, 0, 1, GUILayout.Width(250));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(50);
                GUILayout.Label("Engine Filler (" + (Settings.engineFiller * 100).ToString("F0") + "%):");
                GUILayout.FlexibleSpace();
                Settings.engineFiller = GUILayout.HorizontalSlider(Settings.engineFiller, 0, 1, GUILayout.Width(250));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(50);
                GUILayout.Label("RCS Filler (" + (Settings.rcsFiller * 100).ToString("F0") + "%):");
                GUILayout.FlexibleSpace();
                Settings.rcsFiller = GUILayout.HorizontalSlider(Settings.rcsFiller, 0, 1, GUILayout.Width(250));
                GUILayout.EndHorizontal();
            }

            Settings.doTanks = GUILayout.Toggle(Settings.doTanks, "Include tanks");
            Settings.manned = GUILayout.Toggle(Settings.manned, "Include manned parts");
            Settings.doStock = GUILayout.Toggle(Settings.doStock, "Include stock parts");
            Settings.processManipulableOnly = GUILayout.Toggle(Settings.processManipulableOnly, "Process manipulable-only parts");
            Settings.limitSize = GUILayout.Toggle(Settings.limitSize, "Limit Size");
            Settings.hideUnlessChangesDetected = GUILayout.Toggle(Settings.hideUnlessChangesDetected, "Hide button unless changes detected");

            if (Settings.limitSize)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(50);
                GUILayout.Label("Largest Allowable Part: ");
                var s = GUILayout.TextField(Settings.largestAllowablePart.ToString("F0"), GUILayout.Width(75));
                GUILayout.Label(" liters");
                if (float.TryParse(s, out float f))
                {
                    Settings.largestAllowablePart = f;
                }
                GUILayout.EndHorizontal();
            }

            Settings.stackParts = GUILayout.Toggle(Settings.stackParts, "Allow Stackable Parts");

            if (Settings.stackParts)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(50);
                GUILayout.Label("Max Common Stack Volume: ");
                var t1 = GUILayout.TextField(Settings.maxCommonStackVolume.ToString("F0"), GUILayout.Width(50));
                GUILayout.Label(" liters");
                if (float.TryParse(t1, out float f))
                {
                    Settings.maxCommonStackVolume = f;
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(50);
                GUILayout.Label("Max Parts In Stack: ");
                var t2 = GUILayout.TextField(Settings.maxPartsInStack.ToString(), GUILayout.Width(50));
                GUILayout.Label("");
                if (int.TryParse(t2, out int i))
                {
                    Settings.maxPartsInStack = i;
                }
                GUILayout.EndHorizontal();
            }
            Settings.restart = GUILayout.Toggle(Settings.restart, "Automatically restart if needed");

            GUILayout.Space(20);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Reset to Default"))
            {
                Settings.ResetToDefaults();
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Save & Close", GUILayout.Width(120)))
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