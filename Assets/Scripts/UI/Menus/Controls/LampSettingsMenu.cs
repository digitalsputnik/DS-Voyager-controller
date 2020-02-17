using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using VoyagerApp.Lamps.Voyager;
using VoyagerApp.UI.Overlays;
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

        int updateLampCount;
        int updateFinished;
        bool updatesFinished = true;
        public static bool isFromUpdatePrompt = false;

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

            if (isFromUpdatePrompt && !Debug.isDebugBuild)
            {
                isFromUpdatePrompt = false;
                UpdateSelected();
            }
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
            var lampsNotUpdateable = WorkspaceUtils.SelectedVoyagerLamps.Where(l => l.battery < 30.0).ToList();
            var lampsUpdateable = WorkspaceUtils.SelectedVoyagerLamps.Where(l => l.battery >= 30.0).ToList();

            if (lampsUpdateable.Count > 0)
            {
                VoyagerUpdateUtility utility = new VoyagerUpdateUtility();
                lampsUpdateable.ForEach(lamp => utility.UpdateLamp(lamp,
                                                         OnUpdateFinished,
                                                         OnUpdateMessage));

                updateLampCount = lampsUpdateable.Count;
                updateFinished = 0;
                UpdateInfoText();
                updatesFinished = false;

                overallUpdateInfoText.gameObject.SetActive(true);
                updateText.gameObject.SetActive(true);
            }

            if (lampsNotUpdateable.Count > 0)
            {
                string[] serials = new string[lampsNotUpdateable.Count];
                for (int i = 0; i < lampsNotUpdateable.Count; i++)
                    serials[i] = lampsNotUpdateable[i].serial;

                DialogBox.Show(
                    "NOTICE",
                    $"Some lamps will not be updated due to low battery (below 30%): {string.Join(", ", serials)}",
                    new string[] { "OK" },
                    new Action[] { null });
            }
        }

        void OnUpdateFinished(VoyagerUpdateResponse response)
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
    }
}