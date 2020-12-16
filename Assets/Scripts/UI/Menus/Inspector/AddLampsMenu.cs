using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DigitalSputnik.Bluetooth;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using VoyagerApp.Lamps;
using VoyagerApp.Lamps.Voyager;
using VoyagerApp.Utilities;
using VoyagerApp.Workspace;
using VoyagerApp.Workspace.Views;

namespace VoyagerApp.UI.Menus
{
    public class AddLampsMenu : Menu
    {
        [SerializeField] Transform container = null;
        [SerializeField] GameObject bluetoothBtn = null;
        [SerializeField] GameObject bleInfoText = null;
        [SerializeField] AddLampItem prefab = null;
        [SerializeField] Button addAllLampsBtn = null;
        List<AddLampItem> items = new List<AddLampItem>();
        Vector3 step = new Vector3(0, -1.0f , 0);

        internal override void OnShow()
        {
            LampManager.instance.onLampAdded += OnLampAdded;
            LampManager.instance.onLampBroadcasted += OnLampBroadcasted;
            WorkspaceManager.instance.onItemRemoved += ItemRemovedFromWorkspace;
            WorkspaceManager.instance.onItemAdded += ItemAddedToWorkspace;
            ApplicationState.OnNewProject += NewProject;

            addAllLampsBtn.gameObject.SetActive(false);
            AddLampsToList();

            RemoveWorkspaceLampsFromList();

            bluetoothBtn.SetActive(false);

            if (!BluetoothHelper.IsInitialized)
                BluetoothHelper.Initialize(this, ScanBluetooth);
            else
                ScanBluetooth();

            if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android)
                bleInfoText.SetActive(true);

            StartCoroutine(AddLampsAgain());
        }

        internal override void OnHide()
        {
            LampManager.instance.onLampAdded -= OnLampAdded;
            LampManager.instance.onLampBroadcasted -= OnLampBroadcasted;
            WorkspaceManager.instance.onItemRemoved -= ItemRemovedFromWorkspace;
            WorkspaceManager.instance.onItemAdded -= ItemAddedToWorkspace;
            ApplicationState.OnNewProject -= NewProject;

            foreach (var lamp in new List<AddLampItem>(items))
                RemoveLampItem(lamp, false);

            BluetoothHelper.StopScanningForLamps();

            StopCoroutine(AddLampsAgain());
        }

        void RemoveWorkspaceLampsFromList()
        {
            foreach (var lamp in items.ToArray())
            {
                if (WorkspaceUtils.Lamps.Any(l => l.serial == lamp.lamp.serial))
                    RemoveLampItem(lamp, false);
            }
        }

        private void Update()
        {
            foreach (var item in items.ToArray())
            {
                if (!item.lamp.connected)
                    RemoveLampItem(item, false);
            }
        }

        private void ScanBluetooth()
        {
            BluetoothHelper.StartScanningForLamps(LampScanned);
        }

        private void LampScanned(PeripheralInfo peripheral)
        {
            if (!ValidateBluetoothPeripheral(peripheral.name))
                return;
            
            BluetoothHelper.StopScanningForLamps();
            bluetoothBtn.SetActive(true);
        }

        private static bool ValidateBluetoothPeripheral(string name)
        {
            return !LampManager.instance.Lamps.Any(l => l.serial == name && l.connected);
        }

        void NewProject()
        {
            foreach (var item in items.ToArray())
            {
                RemoveLampItem(item, false);
            }
        }

        IEnumerator AddLampsAgain()
        {
            while (true)
            {
                yield return new WaitForSeconds(0.5f);
                AddLampsToList();
            }
        }

        void ItemRemovedFromWorkspace(WorkspaceItemView item)
        {
            if (item is LampItemView lampView)
                OnLampAdded(lampView.lamp);
        }

        void ItemAddedToWorkspace(WorkspaceItemView item)
        {
            if (item is LampItemView view)
            {
                var addItem = items.FirstOrDefault(v => v.lamp == view.lamp);
                if (addItem != null)
                {
                    RemoveLampItem(addItem, true);
                }
            }
        }

        void AddLampsToList()
        {
            var lamps = WorkspaceUtils.Lamps;
            foreach (var lamp in LampManager.instance.Lamps)
            {
                var voyager = (VoyagerLamp) lamp;

                if (items.All(i => i.lamp.serial != lamp.serial) &&
                    lamps.All(l => l.serial != lamp.serial) &&
                    lamp.connected)
                {
                    if (lamp.version == UpdateSettings.VoyagerAnimationVersion)
                    {
                        if (voyager.dmxPollReceived)
                            OnLampAdded(lamp);
                    }
                    else
                    {
                        OnLampAdded(lamp);
                    }
                }
            }
            CheckForAddAllLampsButton();
        }

        public void AddAllLamps()
        {
            WorkspaceSelection.instance.Clear();

            List<LampItemView> addedLampItems = new List<LampItemView>();

            foreach (var item in items.ToList())
            {
                var lampItem = item.lamp.AddToWorkspace(WorkspaceUtils.PositionOfLastNotSelectedLamp + step);
                addedLampItems.Add(lampItem);
            }

            WorkspaceUtils.SetCameraPosition(WorkspaceUtils.PositionOfLastNotSelectedLamp);

            foreach (var item in addedLampItems)
                WorkspaceSelection.instance.SelectItem(item);
            
            while (items.Count > 0)
                RemoveLampItem(items[0], true);
        }

        public void RemoveLampItem(AddLampItem item, bool closeMenuOnEmpty)
        {
            if (items.Contains(item))
            {
                items.Remove(item);
                Destroy(item.gameObject);
            }

            if (items.Count == 0 && closeMenuOnEmpty)
                GetComponentInParent<MenuContainer>().ShowMenu(null);

            CheckForAddAllLampsButton();
        }

        private void OnLampAdded(Lamp lamp)
        {
            Debug.Log("OnLampAdded: " + lamp.serial);

            if (WorkspaceUtils.Lamps.Any(l => l == lamp) ||
                items.Any(i => i.lamp == lamp) ||
                !lamp.connected ||
                !(math.abs(NetUtils.VoyagerClient.TimeOffset) > 0.01f))
                return;

            Debug.Log("Didn't return");

            var item = Instantiate(prefab, container);
            item.SetLamp(lamp);
            items.Add(item);
            CheckForAddAllLampsButton();
        }

        private void OnLampBroadcasted(Lamp lamp)
        {
            Debug.Log("OnLampBroadcasted: " + lamp.serial);

            if (WorkspaceUtils.Lamps.Any(l => l == lamp))
                return;

            Debug.Log("Didn't return");

            WorkspaceSelection.instance.Clear();
            var vlamp = lamp.AddToWorkspace(WorkspaceUtils.PositionOfLastNotSelectedLamp + step);
            WorkspaceUtils.SetCameraPosition(vlamp.transform.localPosition);
            WorkspaceSelection.instance.SelectItem(vlamp);
        }

        void CheckForAddAllLampsButton()
        {
            addAllLampsBtn.gameObject.SetActive(items.Count > 1);
        }
    }
}