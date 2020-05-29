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
using VoyagerApp.UI.Overlays;

namespace VoyagerApp.UI.Menus
{
    public class BluetoothClientModeMenu : Menu
    {
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

            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                SetupIOS();
                ShowTypeSsid();
                _bleInfoText.SetActive(true);
            }
            else if (Application.platform == RuntimePlatform.Android)
            {
                ShowTypeSsid();
                _bleInfoText.SetActive(true);
            }
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

        //void OnConnectedToLamp(BluetoothConnection connection)
        //{
        //    connection.OnServices += (services) => OnServices(connection, services);
        //    connection.GetServices();
        //}

        //void OnServices(BluetoothConnection connection, string[] services)
        //{
        //    if (services.Any(s => s.ToLower() == BluetoothHelper.SERVICE_UID))
        //    {
        //        connection.OnCharacteristics += (service, characs) => OnCharacteristics(connection, service, characs);
        //        connection.GetCharacteristics(BluetoothHelper.SERVICE_UID);
        //    }
        //    else
        //    {
        //        BluetoothHelper.DisconnectFromPeripheral(connection.ID);
        //    }
        //}

        //void OnLampDisconnected(string id)
        //{
        //    var connection = _connections.FirstOrDefault(c => c.ID == id);
        //    if (connection != null) _connections.Remove(connection);
        //}

        //void OnCharacteristics(BluetoothConnection connection, string service, string[] characs)
        //{
        //    if (service.ToLower() == BluetoothHelper.SERVICE_UID)
        //    {
        //        const string READ_CHAR = BluetoothHelper.UART_RX_CHARACTERISTIC_UUID;
        //        const string WRITE_CHAR = BluetoothHelper.UART_TX_CHARACTERISTIC_UUID;

        //        if (characs.Any(c => c.ToLower() == READ_CHAR) && characs.Any(c => c.ToLower() == WRITE_CHAR))
        //        {
        //            _connections.Add(connection);
        //        }
        //        else
        //        {
        //            BluetoothHelper.DisconnectFromPeripheral(connection.ID);
        //        }
        //    }
        //}

        IEnumerator IEnumSetWifiSettings(string ssid, string password)
        {
            _statusText.SetActive(true);

            const string JSON = @"{""op_code"": ""ble_ack""}";
            const string SERVICE = BluetoothHelper.SERVICE_UID;
            const string READ_CHAR = BluetoothHelper.UART_TX_CHARACTERISTIC_UUID;
            const string WRITE_CHAR = BluetoothHelper.UART_RX_CHARACTERISTIC_UUID;

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
                        var package = VoyagerNetworkMode.Client(ssid, WPA_PSK(ssid, password).ToLower(), active.Name);
                        active.Write(SERVICE, WRITE_CHAR, package.ToData());
                    }

                    yield return new WaitForSeconds(0.2f);
                }

                if (hadError)
                    DialogBox.Show("BLE Error", errorMessage, new string[] { "OK" }, new Action[] { null });
            }

            //List<BluetoothConnection> connections = new List<BluetoothConnection>();
            //List<string> approvedConnections = new List<string>();
            //int waitCount = _ids.Length;

            //foreach (var id in _ids)
            //{
            //    BluetoothHelper.ConnectToPeripheral(id,
            //        (connection) =>
            //        {
            //            connection.OnServices += (services) =>
            //            {
            //                if (services.Any(s => s.ToLower() == SERVICE))
            //                {
            //                    connection.OnCharacteristics += (service, characs) =>
            //                    {
            //                        if (service.ToLower() == SERVICE)
            //                        {
            //                            if (characs.Any(c => c.ToLower() == READ_CHAR) && characs.Any(c => c.ToLower() == WRITE_CHAR))
            //                            {
            //                                connections.Add(connection);
            //                                connection.SubscribeToCharacteristicUpdate(SERVICE, READ_CHAR);
            //                                connection.OnData += (data) =>
            //                                {
            //                                    if (Encoding.UTF8.GetString(data) == JSON)
            //                                    {
            //                                        if (!approvedConnections.Contains(id))
            //                                            approvedConnections.Add(id);
            //                                    }
            //                                };
            //                            }
            //                            else
            //                                BluetoothHelper.DisconnectFromPeripheral(connection.ID);
            //                        }
            //                    };
            //                    connection.GetCharacteristics(BluetoothHelper.SERVICE_UID);
            //                }
            //                else
            //                {
            //                    BluetoothHelper.DisconnectFromPeripheral(connection.ID);
            //                }
            //            };
            //            connection.GetServices();
            //        },
            //        (error) =>
            //        {
            //            Debug.Log($"Failed to connect to {id}");
            //            waitCount--;
            //        },
            //        (id) =>
            //        {
            //            Debug.Log($"Disconnected from {id}");
            //            waitCount--;
            //        }
            //    );
            //}

            //_connections.ForEach(c =>
            //{
            //    c.SubscribeToCharacteristicUpdate(SERVICE, READCHARAC);
            //    c.OnData += (data) =>
            //    {
            //        if (Encoding.UTF8.GetString(data) == JSON)
            //        {
            //            if (!approvedConnections.Contains(c.ID))
            //                approvedConnections.Add(c.ID);
            //        }
            //    };
            //});

            //// TODO: There should be a timeout here!
            //while (approvedConnections.Count != _connections.Count)
            //{
            //    _connections.ForEach(c =>
            //    {
            //        if (!approvedConnections.Contains(c.ID))
            //        {
            //            var package = VoyagerNetworkMode.Client(ssid, password, c.Name);
            //            c.Write(SERVICE, WRITECHARAC, package.ToData());
            //        }
            //    });

            //    yield return new WaitForSeconds(1.0f);
            //}

            GetComponentInParent<InspectorMenuContainer>().ShowMenu(null);
        }

        public string WPA_PSK(string ssid, string password)
        {
            byte[] ssidBytes = Encoding.ASCII.GetBytes(ssid);
            byte[] passwordBytes = Encoding.ASCII.GetBytes(password);
            Rfc2898DeriveBytes pbkdf2;
            //little magic here
            //Rfc2898DeriveBytes class has restriction of salt size to >= 8
            //but rfc2898 not (see http://www.ietf.org/rfc/rfc2898.txt)
            //we use Reflection to setup private field to avoid this restriction

            if (ssid.Length >= 8)
            {
                pbkdf2 = new Rfc2898DeriveBytes(passwordBytes, ssidBytes, 4096);
            }
            else
            {
                //use dummy salt here, we replace it later vie reflection
                pbkdf2 = new Rfc2898DeriveBytes(password, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 }, 4096);

                var saltField = typeof(Rfc2898DeriveBytes).GetField("m_salt", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance);
                saltField.SetValue(pbkdf2, ssidBytes);
            }

            //get 256 bit PMK key
            byte[] resultBytes = pbkdf2.GetBytes(32);
            return BitConverter.ToString(resultBytes).Replace("-", "");
        }

        void SetupIOS()
        {
#if UNITY_IOS
            string ssid = IOSNetworkHelpers.GetCurrentSsidName();
            _ssidField.text = (ssid == "unknown") ? ApplicationSettings.IOSBluetoothWifiSsid : ssid;
            _setSsidScan.interactable = false;
#endif
        }

        void StartLoadingSsids()
        {
            _ssidList.index = 0;
            _setBtn.interactable = false;
            _ssidList.interactable = false;
            _ssidRefreshBtn.interactable = false;

#if UNITY_ANDROID
            AndroidNetworkHelpers.ScanForSsids(this, 10.0f, OnSsidListReceived);
#endif

            _loading = true;

            StartCoroutine(IEnumLoadingAnimation());
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