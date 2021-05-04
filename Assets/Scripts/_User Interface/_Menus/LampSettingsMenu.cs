using System.Collections.Generic;
using System.Linq;
using DigitalSputnik.Voyager;
using UnityEngine;
using UnityEngine.UI;
using VoyagerController.Bluetooth;
using VoyagerController.Workspace;

namespace VoyagerController.UI
{
    public class LampSettingsMenu : Menu
    {
        private const string SELECT_ALL_TEXT = "SELECT ALL";
        private const string DESELECT_ALL_TEXT = "DESELECT ALL";
        
        [Space(3)]
        [SerializeField] private Text _selectDeselectBtnText = null;
        [SerializeField] private GameObject _selectDeselectBtn = null;
        [SerializeField] private GameObject _infoTextObj = null;
        [SerializeField] private GameObject _networkSettingsBtn = null;
        [SerializeField] private GameObject _updateBtn = null;
        [Space(3)]
        [SerializeField] private Text _overallUpdateInfoText = null;
        [SerializeField] private Text _updateText = null;
        [SerializeField] private UpdateDialog updateDialog = null;

        private int _updateLampCount;
        private bool _updatesFinished = true;
        private readonly List<VoyagerLamp> _lampsUpdating = new List<VoyagerLamp>();

        public void SelectDeselect()
        {
            if (!WorkspaceSelection.GetSelected<VoyagerItem>().Any())
                foreach (var voyager in WorkspaceManager.GetItems<VoyagerItem>().ToList())
                    WorkspaceSelection.SelectItem(voyager);
            else
                foreach (var voyager in WorkspaceManager.GetItems<VoyagerItem>().ToList())
                    WorkspaceSelection.DeselectItem(voyager);
        }

        public override void Start()
        {
            base.Start();
            _overallUpdateInfoText.gameObject.SetActive(false);
            _updateText.gameObject.SetActive(false);
        }

        internal override void OnShow()
        {
            WorkspaceSelection.SelectionChanged += OnSelectionChanged;
            WorkspaceManager.ItemAdded += ItemAddedToOrRemovedFromWorkspace;
            WorkspaceManager.ItemRemoved += ItemAddedToOrRemovedFromWorkspace;
            EnableDisableObjects();
        }

        internal override void OnHide()
        {
            WorkspaceSelection.SelectionChanged -= OnSelectionChanged;
            WorkspaceManager.ItemAdded -= ItemAddedToOrRemovedFromWorkspace;
            WorkspaceManager.ItemRemoved -= ItemAddedToOrRemovedFromWorkspace;

            if (_updatesFinished)
            {
                _overallUpdateInfoText.gameObject.SetActive(false);
                _updateText.gameObject.SetActive(false);
            }
        }

        private void OnSelectionChanged() => EnableDisableObjects();

        private void ItemAddedToOrRemovedFromWorkspace(WorkspaceItem item) => EnableDisableObjects();

        private void EnableDisableObjects()
        {
            var anyWorkspace = WorkspaceManager.GetItems<VoyagerItem>().Any();
            var allSelected = WorkspaceUtils.AllLampsSelected;
            var hasSelected = WorkspaceSelection.GetSelected<VoyagerItem>().Any();
            var anyBluetooth = WorkspaceSelection.GetSelected<VoyagerItem>().Any(l => l.LampHandle.Endpoint is BluetoothEndPoint);

            _infoTextObj.SetActive(!hasSelected);
            _networkSettingsBtn.SetActive(anyWorkspace);
            _updateBtn.SetActive(hasSelected && !anyBluetooth);
            _selectDeselectBtnText.text = allSelected ? DESELECT_ALL_TEXT : SELECT_ALL_TEXT;
        }

        public void UpdateSelected()
        {
            /*foreach (var item in WorkspaceSelection.GetSelected<VoyagerItem>())
            {
                var lampHandle = item.LampHandle;

                if (_lampsUpdating.Contains(lampHandle)) continue;
                
                _lampsUpdating.Add(lampHandle);
                VoyagerUpdater.UpdateLamp(lampHandle, OnUpdateFinished, OnUpdateMessage);
            }
            
            UpdateUpdateUI();*/

            var lampsNotUpdateable = WorkspaceSelection.GetSelected<VoyagerItem>().Where(l => l.LampHandle.BatteryLevel < 30.0 && !l.LampHandle.Charging).ToList();
            var lampsUpdateable = WorkspaceSelection.GetSelected<VoyagerItem>().Where(l => l.LampHandle.BatteryLevel >= 30.0 || l.LampHandle.Charging).ToList();

            _lampsUpdating.Clear();

            if (lampsUpdateable.Count > 0)
            {
                lampsUpdateable.ForEach(lamp => 
                {
                    VoyagerUpdater.UpdateLamp(lamp.LampHandle, OnUpdateFinished, OnUpdateMessage);
                    _lampsUpdating.Add(lamp.LampHandle);
                });

                UpdateUpdateUI();
            }

            if (lampsNotUpdateable.Count > 0)
            {
                var lampHandles = new List<VoyagerLamp>();

                lampsNotUpdateable.ForEach(lamp => lampHandles.Add(lamp.LampHandle));

                updateDialog.Show(lampHandles,(lamp) =>
                {
                    VoyagerUpdater.UpdateLamp(lamp, OnUpdateFinished, OnUpdateMessage);
                    _lampsUpdating.Add(lamp);
                    UpdateUpdateUI();
                });
            }
        }

        private void UpdateUpdateUI()
        {
            _updateLampCount = _lampsUpdating.Count;
            UpdateInfoText();
            _updatesFinished = false;

            _overallUpdateInfoText.gameObject.SetActive(true);
            _updateText.gameObject.SetActive(true);
        }

        private void OnUpdateFinished(VoyagerUpdateResponse response)
        {
            if (_lampsUpdating.Contains(response.Lamp))
                _lampsUpdating.Remove(response.Lamp);
            UpdateInfoText();
        }

        private void UpdateInfoText()
        {
            MainThread.Dispatch(() =>
            {
                if (_overallUpdateInfoText != null)
                    _overallUpdateInfoText.text = $"UPDATED: {VoyagerUpdater.UpdatesFinished}/{_updateLampCount}";
            });
        }

        private void OnUpdateMessage(VoyagerUpdateMessage message)
        {
            MainThread.Dispatch(() =>
            {
                if (_updateText != null)
                    _updateText.text = $"{message.Lamp.Serial}\n" +
                                       $"{message.Message.ToUpper()}";
            });
        }
    }
}