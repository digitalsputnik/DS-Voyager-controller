﻿using DigitalSputnik.Bluetooth;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using VoyagerApp.Lamps;
using VoyagerApp.Utilities;
using VoyagerApp.Workspace;
using VoyagerApp.Workspace.Views;

namespace VoyagerApp.UI.Menus
{
    public class AddLampsMenu : Menu
    {
        [SerializeField] ClientModeMenu menu = null;
        [SerializeField] Transform wifiContainer    = null;
        [SerializeField] Transform bleContainer = null;
        [SerializeField] AddLampItem prefab     = null;
        [SerializeField] BLEItem blePrefab = null;
        [SerializeField] Button addAllLampsBtn  = null;
        List<AddLampItem> items = new List<AddLampItem>();

        public List<BLEItem> scannedLamps = new List<BLEItem>();

        bool initialized = false;

        internal override void OnShow()
        {
            LampManager.instance.onLampAdded += OnLampAdded;
            WorkspaceManager.instance.onItemRemoved += ItemRemovedFromWorkspace;
            WorkspaceManager.instance.onItemAdded += ItemAddedToWorkspace;
            ApplicationState.OnNewProject += NewProject;

            if (!initialized)
                BluetoothHelper.Initialize(this, OnInitialized);
            else
                StartScanning();

            addAllLampsBtn.gameObject.SetActive(false);
            OpenWifiList();
            AddLampsToList();

            StartCoroutine(AddLampsAgain());
        }

        internal override void OnHide()
        {
            LampManager.instance.onLampAdded -= OnLampAdded;
            WorkspaceManager.instance.onItemRemoved -= ItemRemovedFromWorkspace;
            WorkspaceManager.instance.onItemAdded -= ItemAddedToWorkspace;
            ApplicationState.OnNewProject -= NewProject;

            foreach (var lamp in new List<AddLampItem>(items))
                RemoveLampItem(lamp);

            StopCoroutine(AddLampsAgain());
            StopScanning();
            ClearCache();
        }

        void OnInitialized()
        {
            Debug.Log("BluetoothLog: Initialized Bluetooth");
            initialized = true;
            StartScanning();
        }

        void StartScanning()
        {
            Debug.Log($"BluetoothLog: Scanning lamps");
            
            BluetoothHelper.StartScanningForLamps(OnScanned);
        }

        void StopScanning()
        {
            Debug.Log($"BluetoothLog: Stopped scanning lamps");
            
            BluetoothHelper.StopScanningForLamps();
        }

        void OnScanned(PeripheralInfo peripheral)
        {
            if (scannedLamps.Any(i => i.peripheral.id == peripheral.id))
            {
                scannedLamps.FirstOrDefault(i => i.peripheral.id == peripheral.id).SetPeripheral(peripheral, this);
            }
            else
            {
                BLEItem item = Instantiate(blePrefab, bleContainer);
                item.SetPeripheral(peripheral, this);
                scannedLamps.Add(item);
                Debug.Log($"BluetoothLog: Scanned Lamp - {peripheral.id} {peripheral.name} {peripheral.rssi}");
            }
        }

        void NewProject()
        {
            foreach (var item in items.ToArray())
            {
                items.Remove(item);
                Destroy(item);
            }
        }

        IEnumerator AddLampsAgain()
        {
            while(true)
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
                    RemoveLampItem(addItem);
            }
        }

        void AddLampsToList()
        {
            var lamps = WorkspaceUtils.Lamps;
            foreach (var lamp in LampManager.instance.Lamps)
            {
                if (!items.Any(i => i.lamp.serial == lamp.serial) && !lamps.Any(l => l.serial == lamp.serial) && lamp.connected)
                    OnLampAdded(lamp);
            }
            CheckForAddAllLampsButton();
        }

        public void AddAllLamps()
        {
            int count = items.Count;
            float2[] points = VectorUtils.ScreenVerticalPositions(count);
            AddLampItem[] itms = items.ToArray();
            for (int i = 0; i < count; i++)
            {
                Vector2 point = points[i];
                AddLampItem item = itms[i];
                item.lamp.AddToWorkspace(point);
            }

            while (items.Count > 0)
                RemoveLampItem(items[0]);
        }

        public void AddAllBleLamps()
        {
            menu.SetupBluetooth(scannedLamps.Where(l => l.selected).ToList());
            GetComponentInParent<InspectorMenuContainer>().ShowMenu(menu);
            addAllLampsBtn.onClick.RemoveAllListeners();
        }

        public void ClearCache()
        {
            for (int i = 0; i < scannedLamps.Count; i++)
                Destroy(scannedLamps[i].gameObject);

            scannedLamps.Clear();
        }

        public void OpenBluetoothList()
        {
            wifiContainer.parent.gameObject.SetActive(false);
            bleContainer.parent.gameObject.SetActive(true);
            addAllLampsBtn.gameObject.GetComponentInChildren<Text>().text = "CONNECT LAMPS";
            addAllLampsBtn.onClick.RemoveAllListeners();
            addAllLampsBtn.onClick.AddListener(AddAllBleLamps);
        }

        public void OpenWifiList()
        {
            bleContainer.parent.gameObject.SetActive(false);
            wifiContainer.parent.gameObject.SetActive(true);
            addAllLampsBtn.gameObject.GetComponentInChildren<Text>().text = "ADD ALL LAMPS";
            addAllLampsBtn.onClick.RemoveAllListeners();
            addAllLampsBtn.onClick.AddListener(AddAllLamps);
        }

        public void RemoveLampItem(AddLampItem item)
        {
            if (items.Contains(item))
            {
                items.Remove(item);
                Destroy(item.gameObject);
            }

            if (items.Count == 0)
                GetComponentInParent<MenuContainer>().ShowMenu(null);

            CheckForAddAllLampsButton();
        }

        void OnLampAdded(Lamp lamp)
        {
            if (!WorkspaceUtils.Lamps.Any(l => l == lamp) && !items.Any(i => i.lamp == lamp) && lamp.connected && math.abs(NetUtils.VoyagerClient.TimeOffset) > 0.01f)
            {
                AddLampItem item = Instantiate(prefab, wifiContainer);
                item.SetLamp(lamp);
                items.Add(item);

                CheckForAddAllLampsButton();
            }
        }

        void CheckForAddAllLampsButton()
        {
            if(wifiContainer.parent.gameObject.activeSelf)
                addAllLampsBtn.gameObject.SetActive(items.Count > 1);
            if (bleContainer.parent.gameObject.activeSelf)
                addAllLampsBtn.gameObject.SetActive(scannedLamps.Where(i => i.selected).Count() > 0);
        }
    }
}