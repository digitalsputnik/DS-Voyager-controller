using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
            // TODO: Ssid textfield saving should work and iOS & android the same way.
            //       Right now it only works on iOS.
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
            _setBtn.interactable = true;

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
            foreach (var connection in _connections)
                BluetoothHelper.DisconnectFromPeripheral(connection.ID);

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
                bool started = false;
                bool connected = false;
                string errorMessage = null;
                BluetoothConnection active = null;

                BluetoothHelper.ConnectAndValidate(id,
                    (connection) =>
                    {
                        active = connection;
                        _connections.Add(active);

                        active.OnData += (data) =>
                        {
                            if (Encoding.UTF8.GetString(data) == JSON)
                            {
                                started = true;
                                BluetoothHelper.DisconnectFromPeripheral(active.ID);
                            }
                        };

                        active.SubscribeToCharacteristicUpdate(SERVICE, READ_CHAR);

                        connected = true;
                    },
                    (err) =>
                    {
                        hadError = true;
                        errorMessage = $"Failed to connect to device {id}";
                        done = true;
                    },
                    (err) =>
                    {
                        if (!string.IsNullOrEmpty(err))
                        {
                            hadError = true;
                            errorMessage = $"Disconnected, got error {err}";
                        }

                        done = true;
                        _connections.Remove(_connections.FirstOrDefault(c => c.ID == id));
                    }
                );

                var endtime = Time.time + _timeout;

                while (!done)
                {
                    if (Time.time >= endtime)
                    {
                        hadError = true;
                        string name = active != null ? active.Name : id;
                        errorMessage = $"Timeout setting {name}.";
                        break;
                    }

                    if (connected && !started && active != null)
                    {
                        var package = VoyagerNetworkMode.SecureClient(ssid, password, active.Name);
                        active.Write(WRITE_CHAR, package.ToData());
                    }

                    yield return new WaitForSeconds(0.5f);
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
            _loading = true;

            StartCoroutine(PollSsidsFromBluetooth());
            StartCoroutine(IEnumLoadingAnimation());
        }

        IEnumerator PollSsidsFromBluetooth()
        {
            List<string[]> all = new List<string[]>();

            int finished = 0;

            for (int i = 0; i < 4; i++)
            {
                if (i < _ids.Length)
                {
                    StartCoroutine(GetSsidFromId(_ids[i], (result) =>
                    {
                        all.Add(result);
                        finished++;
                    }));
                }
            }

            float endTime = Time.time + _timeout;
            while (Time.time < endTime && finished < _ids.Length)
                yield return new WaitForSeconds(0.5f);

            foreach (var connection in _connections)
                BluetoothHelper.DisconnectFromPeripheral(connection.ID);

            string[] ssids = new string[0];

            if (all.Count != 0)
            {
                ssids = all
                    .Skip(1)
                    .Aggregate(new HashSet<string>(all.First()), (h, e) =>
                    {
                        h.IntersectWith(e);
                        return h;
                    })
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Distinct()
                    .ToArray();
            }

            OnSsidListReceived(ssids);
        }

        IEnumerator GetSsidFromId(string id, Action<string[]> callback)
        {
            bool started = false;
            bool finished = false;
            List<string> ssids = new List<string>();

            const string BEGIN_JSON = "{\"op_code\": \"ack_ssid_list_request\"}";
            const string END_JSON = "{\"op_code\": \"ack_ssid_list_complete\"}";

            BluetoothConnection active = null;

            BluetoothHelper.ConnectAndValidate(id,
                (conn) =>
                {
                    active = conn;
                    _connections.Add(active);

                    active.OnData += (data) =>
                    {
                        string decoded = Encoding.UTF8.GetString(data);

                        if (decoded != BEGIN_JSON && decoded != END_JSON)
                        {
                            foreach (var ssid in decoded.Split(','))
                            {
                                if (!ssids.Contains(ssid))
                                    ssids.Add(ssid);
                            }
                        }

                        if (decoded == BEGIN_JSON)
                            started = true;

                        if (decoded == END_JSON)
                            finished = true;
                    };

                    active.SubscribeToCharacteristicUpdate(SERVICE, READ_CHAR);
                },
                (err) =>
                {
                    finished = true;
                },
                (err) =>
                {
                    finished = true;
                    _connections.Remove(_connections.FirstOrDefault(c => c.ID == id));
                });


            float endTime = Time.time + _timeout;

            while (Time.time < endTime && !finished)
            {
                if (!started && active != null)
                {
                    var packet = new SsidListRequestPacket();
                    var sendData = packet.Serialize();
                    active.Write(WRITE_CHAR, sendData);
                }

                yield return new WaitForSeconds(0.5f);
            }

            callback(ssids.ToArray());
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