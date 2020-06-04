﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DigitalSputnik.Bluetooth;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;
using VoyagerApp.Lamps;
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
        List<BluetoothConnection> _connections = new List<BluetoothConnection>();
        Queue<BluetoothLampItem> _namelessItems = new Queue<BluetoothLampItem>();

        internal override void OnShow()
        {
            foreach (var item in _items.ToList())
                DestroyItem(item);

            LampManager.instance.onLampAdded += OnLampAdded;
            BluetoothHelper.StartScanningForLamps(LampScanned);
            StartCoroutine(GetLampNames());
        }

        internal override void OnHide()
        {
            LampManager.instance.onLampAdded += OnLampAdded;
            BluetoothHelper.StopScanningForLamps();
            StopCoroutine(GetLampNames());
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

                    //if (peripheral.name == "")
                    //{
                        item.Name = "Loading..";
                        _namelessItems.Enqueue(item);
                    //}
                    //else
                        //item.Name = peripheral.name;
                }
            }
        }

        IEnumerator GetLampNames()
        {
            const string SERVICE = BluetoothHelper.SERVICE_UID;
            const string READ_CHAR = BluetoothHelper.UART_TX_CHARACTERISTIC_UUID;
            const string WRITE_CHAR = BluetoothHelper.UART_RX_CHARACTERISTIC_UUID;

            float _timeout = 15.0f; 

            while (true)
            {
                if (_namelessItems.Count > 0)
                {
                    var lamp = _namelessItems.Dequeue();
                    bool done = false;
                    bool hadError = false;
                    bool connected = false;
                    string errorMessage = null;
                    BluetoothConnection active = null;

                    BluetoothHelper.ConnectAndValidate(lamp.BluetoothId,
                        (connection) =>
                        {
                            active = connection;
                            _connections.Add(active);

                            active.OnData += (data) =>
                            {
                                JObject obj = JObject.Parse(Encoding.UTF8.GetString(data));
                                MainThread.Dispach(() => _items.FirstOrDefault(i => i.BluetoothId == lamp.BluetoothId).Name = (string)obj["active_ssid"]);
                                BluetoothHelper.DisconnectFromPeripheral(lamp.BluetoothId);
                            };

                            active.SubscribeToCharacteristicUpdate(SERVICE, READ_CHAR);
                            connected = true;
                        },
                        (err) =>
                        {
                            hadError = true;
                            errorMessage = $"Failed to connect to device {lamp.BluetoothId}";
                            done = true;
                            _connections.Remove(_connections.FirstOrDefault(c => c.ID == lamp.BluetoothId));
    
                        },
                        (err) =>
                        {
                            if (!string.IsNullOrEmpty(err))
                            {
                                hadError = true;
                                errorMessage = $"Disconnected, got error {err}";
                            }

                            done = true;
                            _connections.Remove(_connections.FirstOrDefault(c => c.ID == lamp.BluetoothId));
                        }
                    );

                    var endtime = Time.time + _timeout;

                    while (!done)
                    {
                        if (Time.time >= endtime)
                        {
                            hadError = true;
                            string name = active != null ? active.Name : lamp.BluetoothId;
                            errorMessage = $"Timeout setting {name}.";
                            break;
                        }

                        if (connected)
                            active.Write(WRITE_CHAR, new PollRequestPacket().Serialize());

                        yield return new WaitForSeconds(1.0f);
                    }

                    if (hadError)
                        Debug.Log($"Error: {errorMessage}");
                }

                yield return new WaitUntil(() => _namelessItems.Count > 0);
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
            foreach (var connection in _connections)
                BluetoothHelper.DisconnectFromPeripheral(connection.ID);

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