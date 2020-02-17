using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using VoyagerApp.Lamps.Voyager;
using VoyagerApp.Utilities;
using VoyagerApp.Workspace;

namespace VoyagerApp.UI.Menus
{
    public class LampSettingsMenu : Menu
    {
        public static bool isFromUpdatePrompt = false;

        [SerializeField] Text selectDeselectBtnText     = null;
        [SerializeField] GameObject infoTextObj         = null;
        [SerializeField] GameObject networkSettingsBtn  = null;
        [SerializeField] GameObject updateBtn           = null;
        [Space(3)]
        [SerializeField] Text overallUpdateInfoText = null;
        [SerializeField] Text updateText            = null;
        [SerializeField] UpdateDialog updateDialog  = null;

        int updateLampCount;
        bool updatesFinished = true;
        List<VoyagerLamp> lampsUpdating = null;
        VoyagerUpdateUtility utility = null;

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
            lampsUpdating = null;

            var lampsNotUpdateable = WorkspaceUtils.SelectedVoyagerLamps.Where(l => l.battery < 30.0 && !l.charging).ToList();
            var lampsUpdateable = WorkspaceUtils.SelectedVoyagerLamps.Where(l => l.battery >= 30.0 || l.charging).ToList();

            if (lampsUpdateable.Count > 0)
            {
                utility = new VoyagerUpdateUtility();
                lampsUpdateable.ForEach(lamp => utility.UpdateLamp(lamp,
                                                         OnUpdateFinished,
                                                         OnUpdateMessage));
                lampsUpdating = lampsUpdateable;
                UpdateUpdateUI();
            }

            if (lampsNotUpdateable.Count > 0)
            {
                updateDialog.Show(lampsNotUpdateable,(lamp) =>
                {
                    utility.UpdateLamp(lamp, OnUpdateFinished, OnUpdateMessage);
                    lampsUpdating.Add(lamp);
                    UpdateUpdateUI();
                });
            }
        }

        void UpdateUpdateUI()
        {
            updateLampCount = lampsUpdating.Count;
            UpdateInfoText();
            updatesFinished = false;

            overallUpdateInfoText.gameObject.SetActive(true);
            updateText.gameObject.SetActive(true);
        }

        void OnUpdateFinished(VoyagerUpdateResponse response)
        {
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
                        $"{utility.finishedCount}/" +
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