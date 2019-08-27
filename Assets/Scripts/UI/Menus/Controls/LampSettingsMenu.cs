using UnityEngine;
using UnityEngine.UI;
using VoyagerApp.Lamps.Voyager;
using VoyagerApp.Utilities;
using VoyagerApp.Workspace;

namespace VoyagerApp.UI.Menus
{
    public class LampSettingsMenu : Menu
    {
        [SerializeField] Text overallUpdateInfoText = null;
        [SerializeField] Text updateText            = null;

        int updateLampCount;
        int updateFinished;
        bool updatesFinished = true;

        public override void Start()
        {
            base.Start();
            overallUpdateInfoText.gameObject.SetActive(false);
            updateText.gameObject.SetActive(false);
        }

        internal override void OnHide()
        {
            if (updatesFinished)
            {
                overallUpdateInfoText.gameObject.SetActive(false);
                updateText.gameObject.SetActive(false);
            }
        }

        public void UpdateSelected()
        {
            var lamps = WorkspaceUtils.SelectedVoyagerLamps;
            VoyagerUpdateUtility utility = new VoyagerUpdateUtility();
            lamps.ForEach(lamp => utility.UpdateLamp(lamp,
                                                     OnUpdateFinished,
                                                     OnUpdateMessage));

            updateLampCount = lamps.Count;
            updateFinished = 0;
            UpdateInfoText();
            updatesFinished = false;

            overallUpdateInfoText.gameObject.SetActive(true);
            updateText.gameObject.SetActive(true);
        }

        private void OnUpdateFinished(VoyagerUpdateResponse response)
        {
            updateFinished++;
            UpdateInfoText();
        }

        void UpdateInfoText()
        {
            MainThread.Dispach(() =>
            {
                overallUpdateInfoText.text = $"UPDATED:" +
                                             $"{updateFinished}/" +
                                             $"{updateLampCount}";
            });
        }

        void OnUpdateMessage(VoyagerUpdateMessage message)
        {
            MainThread.Dispach(() =>
            {
                updateText.text = $"{message.lamp.serial}\n" +
                                  $"{message.message.ToUpper()}";
            });
        }
    }
}