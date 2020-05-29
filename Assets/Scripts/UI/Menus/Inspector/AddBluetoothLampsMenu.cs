using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DigitalSputnik.Bluetooth;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;
using VoyagerApp.Lamps;
using VoyagerApp.Lamps.Voyager;
using VoyagerApp.Networking.Voyager;

namespace VoyagerApp.UI.Menus
{
    public class AddBluetoothLampsMenu : Menu
    {
        [SerializeField] Transform _itemsContainer = null;
        [SerializeField] BluetoothLampItem _itemPrefab = null;
        [SerializeField] BluetoothClientModeMenu _clientMenu = null;
        [SerializeField] Button _selectAllBtn = null;
        [SerializeField] Button _continueBtn = null;

        List<BluetoothLampItem> _items = new List<BluetoothLampItem>();

        internal override void OnShow()
        {
            foreach (var item in _items.ToList())
                DestroyItem(item);

            LampManager.instance.onLampAdded += OnLampAdded;
            BluetoothHelper.StartScanningForLamps(LampScanned);
        }

        internal override void OnHide()
        {
            LampManager.instance.onLampAdded += OnLampAdded;
            BluetoothHelper.StopScanningForLamps();
        }

        void Update()
        {
            _selectAllBtn.interactable = _items.Count > 0 && !_items.TrueForAll(i => i.Toggled);
            _continueBtn.interactable = _items.Any(i => i.Toggled);
        }

        void OnLampAdded(Lamp lamp)
        {
            var item = _items.FirstOrDefault(i => i.Name == lamp.serial);

            if (item != null)
            {
                DestroyItem(item);
            }
        }

        void LampScanned(PeripheralInfo peripheral)
        {
            if (!LampExists(peripheral.name))
            {
                var item = _items.FirstOrDefault(i => i.BluetoothId == peripheral.id);

                if (item == null)
                {
                    item = Instantiate(_itemPrefab, _itemsContainer);
                    item.BluetoothId = peripheral.id;
                    item.Toggled = false;
                    _items.Add(item);
                }

                item.Name = peripheral.name;
            }
        }

        bool LampExists(string name)
        {
            return LampManager.instance.Lamps.Any(l =>
            {
                return l.serial == name && l.connected;
            });
        }

        void DestroyItem(BluetoothLampItem item)
        {
            _items.Remove(item);
            item.Destroy();
        }

        public void SelectAll()
        {
            foreach (var item in _items) item.Toggled = true;
        }

        public void Continue()
        {
            List<string> ids = new List<string>();
            _items
                .Where(i => i.Toggled)
                .ToList()
                .ForEach(i => ids.Add(i.BluetoothId));
            _clientMenu.ConnectToLamps(ids.ToArray());
            GetComponentInParent<InspectorMenuContainer>()?.ShowMenu(_clientMenu);
        }
    }
}