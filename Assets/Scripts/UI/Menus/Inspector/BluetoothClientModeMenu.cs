using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using VoyagerApp.Lamps.Voyager;
using VoyagerApp.Networking.Voyager;
using VoyagerApp.UI.Overlays;

namespace VoyagerApp.UI.Menus
{
    public class BluetoothClientModeMenu : Menu
    {
        const string SERVICE = BluetoothHelper.SERVICE_UID;
        const string READ_CHAR = BluetoothHelper.UART_TX_CHARACTERISTIC_UUID;
        const string WRITE_CHAR = BluetoothHelper.UART_RX_CHARACTERISTIC_UUID;

        [Space(3)]
        [SerializeField] GameObject _ssidListObj = null;
        [SerializeField] ListPicker _ssidList = null;
        [SerializeField] Button _ssidRefreshBtn = null;
        [Space(3)]
        [SerializeField] GameObject _ssidFieldObj = null;
        [SerializeField] InputField _ssidField = null;
        [SerializeField] Button _setSsidScan = null;
        [Space(3)]
        [SerializeField] InputField _passwordField = null;
        [SerializeField] Button _setBtn = null;
        [SerializeField] GameObject _statusText = null;
        [SerializeField] GameObject _bleInfoText = null;
        [Space(3)]
        [SerializeField] string[] _loadingAnim = null;
        [SerializeField] float _animationSpeed = 0.6f;
        [SerializeField] float _timeout = 10.0f;

        bool _loading;
        string[] _ids;
        List<BluetoothConnection> _connections = new List<BluetoothConnection>();

        internal override void OnShow()
        {
            _statusText.SetActive(false);
            _bleInfoText.SetActive(true);

            ShowTypeSsid();
        }

        internal override void OnHide()
        {
            if (Application.platform == RuntimePlatform.IPhonePlayer)
                ApplicationSettings.IOSBluetoothWifiSsid = _ssidField.text;

            foreach (var connection in _connections)
                BluetoothHelper.DisconnectFromPeripheral(connection.ID);

            StopAllCoroutines();
        }

        public void ShowScanSsids()
        {
            _ssidListObj.gameObject.SetActive(true);
            _ssidFieldObj.gameObject.SetActive(false);
            StartLoadingSsids();
        }

        public void ShowTypeSsid()
        {
            if (!_loading)
            {
                _ssidListObj.gameObject.SetActive(false);
                _ssidFieldObj.gameObject.SetActive(true);
            }
        }

        public void ReloadSsidList()
        {
            StartLoadingSsids();
        }

        public void ConnectToLamps(string[] ids)
        {
            _ids = ids;
        }

        public void Set()
        {
            var ssid = _ssidListObj.activeSelf ? _ssidList.selected : _ssidField.text;
            var password = _passwordField.text;

            if (password.Length >= 8 && ssid.Length != 0 || password.Length == 0 && ssid.Length != 0)
            {
                StartCoroutine(IEnumSetWifiSettings(ssid, password));
            }
            else if (ssid.Length == 0)
            {
                DialogBox.Show(
                    "INVALID SSID",
                    "SSID cannot be empty.",
                    new string[] { "OK" },
                    new Action[] { null }
                );
            }
            else
            {
                DialogBox.Show(
                    "INVALID PASSWORD",
                    "Password length must be at least 8 characters or empty for public WiFi networks.",
                    new string[] { "OK" },
                    new Action[] { null }
                );
            }
        }

        IEnumerator IEnumSetWifiSettings(string ssid, string password)
        {
            _statusText.SetActive(true);

            const string JSON = @"{""op_code"": ""ble_ack""}";

            foreach (var id in _ids)
            {
                bool done = false;
                bool hadError = false;
                string errorMessage = null;
                BluetoothConnection active = null;

                BluetoothHelper.ConnectToPeripheral(id, (connection) =>
                    {
                        _connections.Add(connection);

                        connection.OnServices += (services) =>
                        {
                            if (services.Any(s => s.ToLower() == SERVICE))
                            {
                                connection.OnCharacteristics += (service, characs) =>
                                {
                                    if (service.ToLower() == SERVICE)
                                    {
                                        if (characs.Any(c => c.ToLower() == READ_CHAR) && characs.Any(c => c.ToLower() == WRITE_CHAR))
                                        {
                                            active = connection;
                                            connection.SubscribeToCharacteristicUpdate(SERVICE, READ_CHAR);
                                            connection.OnData += (data) =>
                                            {
                                                if (Encoding.UTF8.GetString(data) == JSON)
                                                    BluetoothHelper.DisconnectFromPeripheral(connection.ID);
                                            };
                                        }
                                        else
                                        {
                                            hadError = true;
                                            errorMessage = $"No correct characteristic found from lamp {connection.Name}.";
                                            BluetoothHelper.DisconnectFromPeripheral(connection.ID);
                                        }
                                    }
                                };
                                connection.GetCharacteristics(BluetoothHelper.SERVICE_UID);
                            }
                            else
                            {
                                hadError = true;
                                errorMessage = $"No correct service found from lamp {connection.Name}.";
                                BluetoothHelper.DisconnectFromPeripheral(connection.ID);
                            }
                        };
                        connection.GetServices();
                    },
                    (error) =>
                    {
                        hadError = true;
                        errorMessage = $"Failed to connect to device {id}";
                        done = true;
                    },
                    (_) =>
                    {
                        done = true;
                        _connections.Remove(_connections.FirstOrDefault(c => c.ID == id));
                    }
                );

                var startTime = Time.time;

                while (!done)
                {
                    if ((Time.time - startTime) >= _timeout)
                    {
                        hadError = true;
                        string name = active != null ? active.Name : id;
                        errorMessage = $"Timeout setting {name}.";
                        break;
                    }

                    if (active != null)
                    {
                        var package = VoyagerNetworkMode.Client(ssid, password, active.Name);
                        active.Write(SERVICE, WRITE_CHAR, package.ToData());
                    }

                    yield return new WaitForSeconds(0.2f);
                }

                if (hadError)
                    DialogBox.Show("BLE Error", errorMessage, new string[] { "OK" }, new Action[] { null });
            }

            GetComponentInParent<InspectorMenuContainer>().ShowMenu(null);
        }

        void StartLoadingSsids()
        {
            _ssidList.index = 0;
            _setBtn.interactable = false;
            _ssidList.interactable = false;
            _ssidRefreshBtn.interactable = false;

            StartCoroutine(PollSsidsFromBluetooth());

            _loading = true;

            StartCoroutine(IEnumLoadingAnimation());
        }

        IEnumerator PollSsidsFromBluetooth()
        {
            yield return null;

            if (_ids.Length == 0)
            {
                OnSsidListReceived(new string[0]);
            }
            else
            {
                var id = _ids[0];

                BluetoothConnection active = null;

                BluetoothHelper.ConnectToPeripheral(id, (connection) =>
                {
                    _connections.Add(connection);

                    connection.OnServices += (services) =>
                    {
                        if (services.Any(s => s.ToLower() == SERVICE))
                        {
                            connection.OnCharacteristics += (service, characs) =>
                            {
                                if (service.ToLower() == SERVICE)
                                {
                                    if (characs.Any(c => c.ToLower() == READ_CHAR) && characs.Any(c => c.ToLower() == WRITE_CHAR))
                                    {
                                        var packet = new SsidListRequestPacket();
                                        var sendData = packet.Serialize();
                                        connection.Write(SERVICE, WRITE_CHAR, sendData);

                                        active = connection;
                                        connection.SubscribeToCharacteristicUpdate(SERVICE, READ_CHAR);
                                        connection.OnData += (data) =>
                                        {
                                            string json = Encoding.UTF8.GetString(data);
                                            DialogBox.Show(
                                                "RECIEVED DATA",
                                                $"from {id}: {json}",
                                                new string[] { "OK", "DISCONNECT" },
                                                new Action[] { null, () => BluetoothHelper.DisconnectFromPeripheral(id) });
                                        };
                                    }
                                    else
                                    {
                                        BluetoothHelper.DisconnectFromPeripheral(id);
                                    }
                                }
                            };
                            connection.GetCharacteristics(BluetoothHelper.SERVICE_UID);
                        }
                        else
                        {
                            BluetoothHelper.DisconnectFromPeripheral(id);
                        }
                    };
                    connection.GetServices();
                },
                (error) =>
                {
                    DialogBox.Show(
                        "CONNECTION FAILED",
                        $"Failed to connect to {id} to ask for ssid list",
                        new string[] { "OK" },
                        new Action[] { null });
                },
                (_) =>
                {
                    _connections.Remove(_connections.FirstOrDefault(c => c.ID == id));
                });
            }
        }

        IEnumerator IEnumLoadingAnimation()
        {
            int i = 0;
            while (_loading)
            {
                _ssidList.SetItems(_loadingAnim[i]);
                yield return new WaitForSeconds(_animationSpeed);
                if (++i >= _loadingAnim.Length)
                    i = 0;
            }
        }

        void OnSsidListReceived(string[] ssids)
        {
            _loading = false;

            if (ssids.Length > 0)
            {
                _setBtn.interactable = true;
                _ssidList.interactable = true;
                _ssidRefreshBtn.interactable = true;
                _ssidList.SetItems(ssids);
            }
            else
            {
                _ssidList.SetItems("Not found");
            }
        }
    }
}