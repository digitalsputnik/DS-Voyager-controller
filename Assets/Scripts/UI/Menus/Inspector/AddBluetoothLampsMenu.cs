using System.Collections.Generic;
using System.Linq;
using DigitalSputnik.Bluetooth;
using UnityEngine;
using VoyagerApp.Lamps;

namespace VoyagerApp.UI.Menus
{
    public class AddBluetoothLampsMenu : Menu
    {
        [SerializeField] Transform _itemsContainer = null;
        [SerializeField] BluetoothLampItem _itemPrefab = null;
        [SerializeField] BluetoothClientModeMenu _clientMenu = null;

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
            return LampManager.instance.Lamps.Any(l => l.serial == name);
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
            _clientMenu.SetBluetoothIds(ids.ToArray());
            GetComponentInParent<InspectorMenuContainer>()?.ShowMenu(_clientMenu);
        }
    }
}