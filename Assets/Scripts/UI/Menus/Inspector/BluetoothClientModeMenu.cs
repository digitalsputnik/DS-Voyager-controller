using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using VoyagerApp.Effects;
using VoyagerApp.Lamps;
using VoyagerApp.Lamps.Voyager;
using VoyagerApp.Networking.Voyager;
using VoyagerApp.UI.Overlays;
using VoyagerApp.Utilities;
using VoyagerApp.Workspace;
using VoyagerApp.Workspace.Views;

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
        [SerializeField] Button _typeSsid = null;
        [Space(3)]
        [SerializeField] InputField _passwordField = null;
        [SerializeField] Button _setBtn = null;
        [SerializeField] Text _setBtnText = null;
        [SerializeField] GameObject _statusText = null;
        [SerializeField] GameObject _bleInfoText = null;
        [SerializeField] AddBluetoothLampsMenu _bleMenu = null;
        [Space(3)]
        [SerializeField] string[] _loadingAnim = null;
        [SerializeField] float _animationSpeed = 0.6f;
        [SerializeField] float _timeout = 30.0f;

        bool _ssidListLoading;
        bool _setLoading;
        string[] _ids;

        List<BluetoothConnection> _connections = new List<BluetoothConnection>();

        internal override void OnShow()
        {
            if (PlayerPrefs.HasKey("last_ble_ssid"))
                _ssidField.text = PlayerPrefs.GetString("last_ble_ssid");

            _statusText.SetActive(false);
            _bleInfoText.SetActive(true);

            ShowTypeSsid();

            _passwordField.text = ApplicationSettings.PreviousWifiPassword;
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

            _ssidListLoading = false;
            _setLoading = false;
        }

        public void Back()
        {
            GetComponentInParent<InspectorMenuContainer>()?.ShowMenu(_bleMenu);
        }

        public void ShowScanSsids()
        {
            _ssidListObj.gameObject.SetActive(true);
            _ssidFieldObj.gameObject.SetActive(false);
            StartLoadingSsids();
        }

        public void ShowTypeSsid()
        {
            if (!_ssidListLoading)
            {
                _ssidField.interactable = true;
                _setSsidScan.interactable = true;
                _passwordField.interactable = true;
                _setBtn.interactable = true;
                _setBtnText.text = "SET";
                _ssidListObj.gameObject.SetActive(false);
                _ssidFieldObj.gameObject.SetActive(true);
            }
        }

        public void ReloadSsidList()
        {
            if (!_ssidListLoading) StartLoadingSsids();
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
                StartSetLoading();
                StartCoroutine(IEnumSetWifiSettings(ssid, password));
                PlayerPrefs.SetString("last_ble_ssid", ssid);
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

            ApplicationSettings.PreviousWifiPassword = password;
        }

        IEnumerator IEnumSetWifiSettings(string ssid, string password)
        {
            _statusText.SetActive(true);

            const string JSON_SET_RESPONSE = @"{""op_code"": ""ble_ack""}";
            const string JSON_SERIAL_REQUEST = @"{""op_code"": ""get_serial""}";
            const string JSON_VERSION_REQUEST = @"{""op_code"": ""get_chip_version""}";
            const string JSON_LENGTH_REQUEST = @"{""op_code"": ""get_length""}";
            const string JSON_BATTERY_REQUEST = @"{""op_code"": ""get_battery""}";
            const string JSON_IP_REQUEST = @"{""op_code"": ""get_ip""}";

            foreach (var id in _ids)
            {
                var done = false;
                var hadError = false;
                string errorMessage = null;
                BluetoothConnection active = null;

                BluetoothHelper.ConnectAndValidate(id,
                    (connection) =>
                    {
                        active = connection;
                        _connections.Add(active);
                        
                        Debug.Log("Connected to ble");

                        active.OnData += data =>
                        {
                            Debug.Log("received:\n" + Encoding.UTF8.GetString(data));
                            
                            if (Encoding.UTF8.GetString(data) == JSON_SET_RESPONSE)
                            {
                                done = true;
                                AddConnectionToWorkspace(active);
                                BluetoothHelper.DisconnectFromPeripheral(active.ID);
                            }
                            else
                            {
                                var packet = Packet.Deserialize<BlePollReply>(data);
                                Debug.Log(packet.Op);
                                switch (packet.Op)
                                {
                                    case "get_serial":
                                        active.Serial = packet.Serial;
                                        break;
                                    case "get_chip_version":
                                        active.Version = packet.Version;
                                        break;
                                    case "get_length":
                                        active.Length = packet.Length;
                                        break;
                                    case "get_battery":
                                        active.Battery = packet.Battery;
                                        break;
                                    case "get_ip":
                                        active.IpAddress = packet.IpAddress;
                                        break;
                                }
                            }
                        };

                        active.SubscribeToCharacteristicUpdate(SERVICE, READ_CHAR);
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

                        _connections.Remove(_connections.FirstOrDefault(c => c.ID == id));
                        done = true;
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

                    if (active != null)
                    {
                        if (!ConnectionHasAllInfo(active))
                        {
                            Debug.Log($"Sending out poll requests");

                            if (string.IsNullOrWhiteSpace(active.Serial))
                            {
                                active.Write(WRITE_CHAR, Encoding.UTF8.GetBytes(JSON_SERIAL_REQUEST));
                                yield return new WaitForSeconds(0.2f);
                            }
                             
                            if (active.Version == null)
                            {
                                active.Write(WRITE_CHAR, Encoding.UTF8.GetBytes(JSON_VERSION_REQUEST));
                                yield return new WaitForSeconds(0.2f);
                            }

                            if (active.Length == -1)
                            {
                                active.Write(WRITE_CHAR, Encoding.UTF8.GetBytes(JSON_LENGTH_REQUEST));
                                yield return new WaitForSeconds(0.2f);
                            }

                            if (active.Battery == -1)
                            {
                                active.Write(WRITE_CHAR, Encoding.UTF8.GetBytes(JSON_BATTERY_REQUEST));
                                yield return new WaitForSeconds(0.2f);
                            }

                            if (active.IpAddress == null)
                            {
                                active.Write(WRITE_CHAR, Encoding.UTF8.GetBytes(JSON_IP_REQUEST));
                                yield return new WaitForSeconds(0.2f);
                            }
                        }
                        else if (active.Version[1] < 500)
                            active.Write(WRITE_CHAR, VoyagerNetworkMode.Client(ssid, password, active.Name).ToData());
                        else
                            active.Write(WRITE_CHAR, VoyagerNetworkMode.SecureClient(ssid, password, active.Name).ToData());
                    }

                    yield return new WaitForSeconds(1f);
                }

                if (hadError)
                    DialogBox.Show("BLE Error", errorMessage, new[] { "OK" }, new Action[] { null });
            }

            GetComponentInParent<InspectorMenuContainer>().ShowMenu(null);
        }

        private bool ConnectionHasAllInfo(BluetoothConnection connection)
        {
            return connection.Length != -1 &&
                   connection.Battery != -1 &&
                   !string.IsNullOrWhiteSpace(connection.Serial) &&
                   connection.IpAddress != null &&
                   connection.Version != null;
        }

        private void AddConnectionToWorkspace(BluetoothConnection connection)
        {
            MainThread.Dispach(() =>
            {
                var lamp = LampManager.instance.GetLampWithSerial(connection.Serial);

                if (lamp == null)
                {
                    lamp = new VoyagerLamp
                    {
                        serial = connection.Serial,
                        chipVersion = connection.Version,
                        battery = connection.Battery,
                        length = connection.Length,
                        address = new IPAddress(connection.IpAddress),
                        lastMessage = 0.0
                    };

                    LampManager.instance.AddLamp(lamp);
                }

                if (lamp is VoyagerLamp voyager)
                {
                    if (!WorkspaceUtils.VoyagerLamps.Contains(voyager))
                    {
                        var item = WorkspaceManager.instance.InstantiateItem<VoyagerItemView>(voyager);
                        item.StartCoroutine(ApplyEffectWhenConnects(voyager));
                    }
                } 
            });
        }

        private IEnumerator ApplyEffectWhenConnects(VoyagerLamp voyager)
        {
            yield return new WaitUntil(() => voyager.connected);

            if (WorkspaceUtils.VoyagerLamps.Contains(voyager))
            {
                var effect = EffectManager.GetEffectWithName<Video>("white");
                voyager.effect = null;
                voyager.SetEffect(effect);
            }
        }

        void StartLoadingSsids()
        {
            _ssidList.index = 0;
            _setBtn.interactable = false;
            _ssidList.interactable = false;
            _ssidRefreshBtn.interactable = false;
            _typeSsid.interactable = false;
            _ssidListLoading = true;

            StartCoroutine(PollSsidsFromBluetooth());
            StartCoroutine(IEnumSsidLoadingAnimation());
        }

        void StartSetLoading()
        {
            _setBtn.interactable = false;
            _typeSsid.interactable = false;
            _ssidField.interactable = false;
            _setSsidScan.interactable = false;
            _passwordField.interactable = false;
            _ssidList.interactable = false;
            _ssidRefreshBtn.interactable = false;

            _setLoading = true;

            StartCoroutine(IEnumSetLoadingAnimation());
        }

        IEnumerator PollSsidsFromBluetooth()
        {
            List<string> supportedLamps = new List<string>();
            List<string> unsupportedLamps = new List<string>();

            foreach (var lamp in _ids)
            {
                bool done = false;
                string errorMessage = "";
                BluetoothConnection active = null;

                float endtime = Time.time + _timeout;

                Connect();

                void Connect()
                {
                    BluetoothHelper.ConnectAndValidate(lamp,
                        (connection) =>
                        {
                            active = connection;
                            MainThread.Dispach(() => endtime = Time.time + 5.0f);

                            _connections.Add(connection);

                            active.OnData = (data) =>
                            {
                                var packet = Packet.Deserialize<BleChipVersion>(data);

                                if (packet != null)
                                    supportedLamps.Add(lamp);
                                else
                                    unsupportedLamps.Add(lamp);

                                done = true;
                                BluetoothHelper.DisconnectFromPeripheral(active.ID);
                            };

                            active.SubscribeToCharacteristicUpdate(SERVICE, READ_CHAR);
                        },
                        (err) => { errorMessage = "Error: failed - " + err; Connect(); },
                        (err) =>
                        {
                            errorMessage = "Disconnected - " + err;
                            _connections.Remove(_connections.FirstOrDefault(c => c.ID == lamp));
                        }
                    );
                }

                while (Time.time < endtime && !done)
                {
                    if (active != null)
                    {
                        active.Write(WRITE_CHAR, new BleChipVersion().Serialize());
                        active.Write(WRITE_CHAR, new PollRequestPacket().Serialize());
                    }

                    yield return new WaitForSeconds(1.0f);
                }

                if (!done)
                    unsupportedLamps.Add(lamp);

                if (errorMessage != "")
                    Debug.Log(errorMessage);
            }

            var ssids = new List<string>();

            if (supportedLamps.Count() == 0)
            {
                DialogBox.Show("BLE Error", "Scanning SSID's failed, make sure you're lamps are updated and try again or type SSID manually.", new string[] { "OK" }, new Action[] { null });
                yield return new WaitForSeconds(0.1f);
                OnSsidListReceived(ssids.ToArray());
            }
            else
            {
                string[] ssidList = new string[0];

                bool finished = false;

                for (int i = 0; i < 4; i++)
                {
                    if (i < supportedLamps.Count())
                    {
                        StartCoroutine(GetSsidFromId(supportedLamps[i], (result) =>
                        {
                            ssidList = result;
                            finished = true;
                        }));
                    }
                }

                float endTime = Time.time + _timeout;
                while (Time.time < endTime && !finished)
                    yield return new WaitForSeconds(0.5f);

                foreach (var connection in _connections)
                    BluetoothHelper.DisconnectFromPeripheral(connection.ID);

                if (ssidList.Length != 0)
                {
                    foreach (var ssid in ssidList)
                    {
                        if (!ssids.Contains(ssid))
                            ssids.Add(ssid);
                    }
                }

                OnSsidListReceived(ssids.ToArray());
            }
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

                    active.OnData = (data) =>
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

        IEnumerator IEnumSsidLoadingAnimation()
        {
            int i = 0;
            while (_ssidListLoading)
            {
                _ssidList.SetItems(_loadingAnim[i]);
                yield return new WaitForSeconds(_animationSpeed);
                if (++i >= _loadingAnim.Length)
                    i = 0;
            }
        }

        void OnSsidListReceived(string[] ssids)
        {
            _ssidListLoading = false;

            if (ssids.Length > 0)
            {
                _setBtn.interactable = true;
                _ssidList.interactable = true;
                _ssidRefreshBtn.interactable = true;
                _typeSsid.interactable = true;
                _ssidList.SetItems(ssids);
            }
            else
            {
                _ssidRefreshBtn.interactable = true;
                _typeSsid.interactable = true;
                _ssidList.SetItems("Not found");
            }
        }

        IEnumerator IEnumSetLoadingAnimation()
        {
            int i = 0;
            while (_setLoading)
            {
                _setBtnText.text = _loadingAnim[i];
                yield return new WaitForSeconds(_animationSpeed);
                if (++i >= _loadingAnim.Length)
                    i = 0;
            }
        }
    }
}