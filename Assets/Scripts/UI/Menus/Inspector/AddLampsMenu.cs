using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DigitalSputnik.Bluetooth;
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
        [SerializeField] Transform container = null;
        [SerializeField] GameObject bluetoothBtn = null;
        [SerializeField] AddLampItem prefab = null;
        [SerializeField] Button addAllLampsBtn = null;
        List<AddLampItem> items = new List<AddLampItem>();

        internal override void OnShow()
        {
            LampManager.instance.onLampAdded += OnLampAdded;
            WorkspaceManager.instance.onItemRemoved += ItemRemovedFromWorkspace;
            WorkspaceManager.instance.onItemAdded += ItemAddedToWorkspace;
            ApplicationState.OnNewProject += NewProject;

            addAllLampsBtn.gameObject.SetActive(false);
            AddLampsToList();

            bluetoothBtn.SetActive(false);

            if (!BluetoothHelper.IsInitialized)
                BluetoothHelper.Initialize(this, ScanBluetooth);
            else
                ScanBluetooth();

            StartCoroutine(AddLampsAgain());
        }

        internal override void OnHide()
        {
            LampManager.instance.onLampAdded -= OnLampAdded;
            WorkspaceManager.instance.onItemRemoved -= ItemRemovedFromWorkspace;
            WorkspaceManager.instance.onItemAdded -= ItemAddedToWorkspace;
            ApplicationState.OnNewProject -= NewProject;

            foreach (var lamp in new List<AddLampItem>(items))
                RemoveLampItem(lamp, false);

            BluetoothHelper.StopScanningForLamps();

            StopCoroutine(AddLampsAgain());
        }

        void ScanBluetooth()
        {
            BluetoothHelper.StartScanningForLamps(LampScanned);
        }

        void LampScanned(PeripheralInfo peripheral)
        {
            if (ValidateBluetoothPeripheral(peripheral.name))
            {
                BluetoothHelper.StopScanningForLamps();
                bluetoothBtn.SetActive(true);
            }
        }

        bool ValidateBluetoothPeripheral(string name)
        {
            return !LampManager.instance.Lamps.Any(l =>
            {
                return l.serial == name && l.connected;
            });
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
                    RemoveLampItem(addItem, true);
            }
        }

        void AddLampsToList()
        {
            var lamps = WorkspaceUtils.Lamps;
            foreach (var lamp in LampManager.instance.Lamps)
            {
                if (!items.Any(i => i.lamp.serial == lamp.serial) &&
                    !lamps.Any(l => l.serial == lamp.serial) &&
                    lamp.connected)
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

        void OnLampAdded(Lamp lamp)
        {
            if (!WorkspaceUtils.Lamps.Any(l => l == lamp) &&
                !items.Any(i => i.lamp == lamp) &&
                lamp.connected &&
                math.abs(NetUtils.VoyagerClient.TimeOffset) > 0.01f)
            {
                AddLampItem item = Instantiate(prefab, container);
                item.SetLamp(lamp);
                items.Add(item);
                CheckForAddAllLampsButton();
            }
        }

        void CheckForAddAllLampsButton()
        {
            addAllLampsBtn.gameObject.SetActive(items.Count > 1);
        }
    }
}