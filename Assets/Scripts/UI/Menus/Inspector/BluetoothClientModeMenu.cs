﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        [Space(3)]
        [SerializeField] string[] _loadingAnim = null;
        [SerializeField] float _animationSpeed = 0.6f;

        List<BluetoothConnection> _connections = new List<BluetoothConnection>();
        bool _loading;

        internal override void OnShow()
        {
            _statusText.SetActive(false);

            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                SetupIOS();
                ShowTypeSsid();
            }
            else if (Application.platform == RuntimePlatform.Android)
            {
                ShowScanSsids();
            }
        }

        internal override void OnHide()
        {
            if (Application.platform == RuntimePlatform.IPhonePlayer)
                ApplicationSettings.IOSBluetoothWifiSsid = _ssidField.text;

            _connections.ForEach(c => BluetoothHelper.DisconnectFromPeripheral(c.ID));
            _connections.Clear();

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
            foreach (var id in ids)
                BluetoothHelper.ConnectToPeripheral(id, OnConnectedToLamp, null, (_) => OnLampDisconnected(id));
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

        void OnConnectedToLamp(BluetoothConnection connection)
        {
            connection.OnServices += (services) => OnServices(connection, services);
            connection.GetServices();
        }

        void OnServices(BluetoothConnection connection, string[] services)
        {
            if (services.Any(s => s.ToLower() == BluetoothHelper.SERVICE_UID))
            {
                connection.OnCharacteristics += (service, characs) => OnCharacteristics(connection, service, characs);
                connection.GetCharacteristics(BluetoothHelper.SERVICE_UID);
            }
            else
            {
                BluetoothHelper.DisconnectFromPeripheral(connection.ID);
            }
        }

        void OnLampDisconnected(string id)
        {
            var connection = _connections.FirstOrDefault(c => c.ID == id);
            if (connection != null) _connections.Remove(connection);
        }

        void OnCharacteristics(BluetoothConnection connection, string service, string[] characs)
        {
            if (service.ToLower() == BluetoothHelper.SERVICE_UID)
            {
                const string READ_CHAR = BluetoothHelper.UART_RX_CHARACTERISTIC_UUID;
                const string WRITE_CHAR = BluetoothHelper.UART_TX_CHARACTERISTIC_UUID;

                if (characs.Any(c => c.ToLower() == READ_CHAR) && characs.Any(c => c.ToLower() == WRITE_CHAR))
                {
                    _connections.Add(connection);
                }
                else
                {
                    BluetoothHelper.DisconnectFromPeripheral(connection.ID);
                }
            }
        }

        IEnumerator IEnumSetWifiSettings(string ssid, string password)
        {
            _statusText.SetActive(true);

            const string JSON = @"{""op_code"": ""ble_ack""}";
            const string SERVICE = BluetoothHelper.SERVICE_UID;
            const string READCHARAC = BluetoothHelper.UART_TX_CHARACTERISTIC_UUID;
            const string WRITECHARAC = BluetoothHelper.UART_RX_CHARACTERISTIC_UUID;

            List<string> approvedConnections = new List<string>();

            _connections.ForEach(c =>
            {
                c.SubscribeToCharacteristicUpdate(SERVICE, READCHARAC);
                c.OnData += (data) =>
                {
                    if (Encoding.UTF8.GetString(data) == JSON)
                    {
                        if (!approvedConnections.Contains(c.ID))
                            approvedConnections.Add(c.ID);
                    }
                };
            });

            // TODO: There should be a timeout here!
            while (approvedConnections.Count != _connections.Count)
            {
                _connections.ForEach(c =>
                {
                    if (!approvedConnections.Contains(c.ID))
                    {
                        var package = VoyagerNetworkMode.Client(ssid, password, c.Name);
                        c.Write(SERVICE, WRITECHARAC, package.ToData());
                    }
                });

                yield return new WaitForSeconds(1.0f);
            }

            GetComponentInParent<InspectorMenuContainer>().ShowMenu(null);
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