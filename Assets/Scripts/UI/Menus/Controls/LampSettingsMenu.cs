using UnityEngine;
using UnityEngine.UI;
using VoyagerApp.Lamps.Voyager;
using VoyagerApp.Utilities;
using VoyagerApp.Workspace;

namespace VoyagerApp.UI.Menus
{
    public class LampSettingsMenu : Menu
    {
        [SerializeField] Text selectDeselectBtnText     = null;
        [SerializeField] GameObject infoTextObj         = null;
        [SerializeField] GameObject networkSettingsBtn  = null;
        [SerializeField] GameObject updateBtn           = null;
        [Space(3)]
        [SerializeField] Text overallUpdateInfoText = null;
        [SerializeField] Text updateText            = null;
        [SerializeField] Text lampInfoText          = null;

        int updateLampCount;
        int updateFinished;
        bool updatesFinished = true;

        public void SelectDeselect()
        {
            if (!WorkspaceUtils.AllLampsSelected)
                WorkspaceUtils.SelectAll();
            else
                WorkspaceUtils.DeselectAll();
        }

        public override void Start()
        {
            base.Start();
            overallUpdateInfoText.gameObject.SetActive(false);
            updateText.gameObject.SetActive(false);
        }

        internal override void OnShow()
        {
            WorkspaceSelection.instance.onSelectionChanged += OnSelectionChanged;
            EnableDisableObjects();
            ShowLampInfo();
        }

        internal override void OnHide()
        {
            WorkspaceSelection.instance.onSelectionChanged -= OnSelectionChanged;

            if (updatesFinished)
            {
                overallUpdateInfoText.gameObject.SetActive(false);
                updateText.gameObject.SetActive(false);
            }
        }

        void OnSelectionChanged()
        {
            EnableDisableObjects();
            ShowLampInfo();
        }

        void EnableDisableObjects()
        {
            bool one = WorkspaceUtils.AtLastOneLampSelected;
            bool all = WorkspaceUtils.AllLampsSelected;

            infoTextObj.SetActive(!one);
            networkSettingsBtn.SetActive(one);
            updateBtn.SetActive(one);

            selectDeselectBtnText.text = all ? "DESELECT ALL" : "SELECT ALL";
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
                if (overallUpdateInfoText != null)
                {
                    overallUpdateInfoText.text =
                        $"UPDATED:" +
                        $"{updateFinished}/" +
                        $"{updateLampCount}";
                }
            });
        }

        void OnUpdateMessage(VoyagerUpdateMessage message)
        {
            MainThread.Dispach(() =>
            {
                if (updateText != null)
                {
                    updateText.text =
                        $"{message.lamp.serial}\n" +
                        $"{message.message.ToUpper()}";
                }
            });
        }

        void ShowLampInfo()
        {
            if (WorkspaceUtils.SelectedVoyagerLamps.Count == 1)
            {
                VoyagerLamp lamp = WorkspaceUtils.SelectedVoyagerLamps[0];
                lampInfoText.text =
                    $"IP: {lamp.address}\n" +
                    $"BATTERY: {lamp.battery}%\n" +
                    $"VERSION: {lamp.version}";
                lampInfoText.gameObject.SetActive(true);
            }
            else
            {
                lampInfoText.text = "";
                lampInfoText.gameObject.SetActive(false);
            }
        }
    }
}