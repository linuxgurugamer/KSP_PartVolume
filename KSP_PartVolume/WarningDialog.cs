using UnityEngine;


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
                dialogMsg = "Part Volume setting have been changed.  The game needs to be restarted for the changes to take effect";
            string windowTitle = "WARNING";

            DialogGUIBase[] options = { new DialogGUIButton("OK", HideCancel) };

            MultiOptionDialog confirmationBox = new MultiOptionDialog("Cargo Changes", dialogMsg, windowTitle, HighLogic.UISkin, options);

            popup = PopupDialog.SpawnPopupDialog(confirmationBox, false, HighLogic.UISkin);
        }

        public void HideCancel()
        {
            InputLockManager.RemoveControlLock("KSPPartVolume");
        }

    }
}
