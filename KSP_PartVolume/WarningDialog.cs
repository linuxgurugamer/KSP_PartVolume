using UnityEngine;
using System.IO;

namespace KSP_PartVolume
{

    public partial class PartVolume : MonoBehaviour
    {
        private PopupDialog popup;

        public void ShowWarning(int i = 0)
        {
            InputLockManager.SetControlLock(ControlTypes.All, "KSPPartVolume");
            string dialogMsg = "";
                if (i > 0)
                    dialogMsg = i + " parts have had their cargo settings changed.  The game needs to be restarted for the changes to take effect";
                else
                    dialogMsg = "Part Volume settings have been changed.  The game needs to be restarted for the changes to take effect";

            string windowTitle = "WARNING";

            DialogGUIBase[] options = { new DialogGUIButton("OK (settings will NOT be applied)", HideCancel), new DialogGUIButton("OK to exit", OkToExit) };

            MultiOptionDialog confirmationBox = new MultiOptionDialog("Cargo Changes", dialogMsg, windowTitle, HighLogic.UISkin, options);

            popup = PopupDialog.SpawnPopupDialog(confirmationBox, false, HighLogic.UISkin);
        }

        public void ShowKIFAWarning(string msg)
        {
            InputLockManager.SetControlLock(ControlTypes.All, "KSPPartVolume");
            string                dialogMsg = msg;
            string windowTitle = "WARNING";

            DialogGUIBase[] options = { new DialogGUIButton("OK to delete and exit", DeleteKIFA), new DialogGUIButton("Cancel", OkToExit) };

            MultiOptionDialog confirmationBox = new MultiOptionDialog("Cargo Changes", dialogMsg, windowTitle, HighLogic.UISkin, options);

            popup = PopupDialog.SpawnPopupDialog(confirmationBox, false, HighLogic.UISkin);
        }

        public void DeleteKIFA()
        {
            HideCancel();
            File.Delete(KIFA);
            OkToExit();
        }
        public void HideCancel()
        {
            InputLockManager.RemoveControlLock("KSPPartVolume");
        }

        public void OkToExit()
        {
            Application.Quit();
            if (Settings.restart)
                StartNewGame();
        }
    }
}
