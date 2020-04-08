﻿#if UNITY_ANDROID

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Android;

namespace DigitalSputnik.Bluetooth
{
    internal class AndroidBluetoothInterface : IBluetoothInterface
    {
        const string LIBRARY_NAME = "com.example.bleplugin";

        AndroidJavaObject _pluginObject;
        AndroidBluetoothListener _listener;

        InternalPeripheralScanHandler _onPeripheralScanned;
        InternalPeripheralConnectHandler _onConnect;
        InternalPeripheralConnectFailHandler _onConnectFail;
        InternalPeripheralDisconnectHandler _onDisconnect;
        InternalServicesHandler _onServices;
        InternalCharacteristicHandler _onCharacteristics;
        InternalCharacteristicUpdateHandler _onCharacteristicUpdate;

        bool _scanning;

        List<ScannedAndroidDevice> _scannedDevices = new List<ScannedAndroidDevice>();
        List<ScannedAndroidDevice> _connectedDevices = new List<ScannedAndroidDevice>();

        public void Initialize()
        {
            if (!Permission.HasUserAuthorizedPermission(Permission.CoarseLocation))
                Permission.RequestUserPermission(Permission.CoarseLocation);
            if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
                Permission.RequestUserPermission(Permission.FineLocation);

            _listener = new GameObject("Android Bluetooth Listener").AddComponent<AndroidBluetoothListener>();
            _listener.StartCoroutine(AfterAuthorization());
        }

        IEnumerator AfterAuthorization()
        {
            yield return new WaitUntil(() =>
                Permission.HasUserAuthorizedPermission(Permission.CoarseLocation) &&
                Permission.HasUserAuthorizedPermission(Permission.CoarseLocation));

            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject context = activity.Call<AndroidJavaObject>("getApplicationContext");

            _pluginObject = new AndroidJavaObject(LIBRARY_NAME + ".BLEObject", context);
            _listener._plugin = _pluginObject;

            SetCallbacks();
        }

        public void SetCallbacks()
        {
            var scanCallback = new AndroidScanResultCallback(OnPeripheralScanned);
            var onConnectionChanged = new AndroidConnectionChangedCallback(PeripheralConnectionStateChanged);
            var onServices = new AndroidServiceCallback(OnServices);
            var onCharacteristics = new AndroidCharacteristicCallback(OnCharacteristics);
            var onCharacteristicsUpdate = new AndroidCharacteristicUpdateCallback(OnCharacteristicUpdate);

            object[] parameters = { scanCallback, onCharacteristics, onCharacteristicsUpdate, onConnectionChanged, onServices };
            _pluginObject.Call("setCallbacks", parameters);
        }

        public void StartScanning(string[] services, InternalPeripheralScanHandler callback)
        {
            if (!_scanning)
            {   
                if (services == null)
                    services = new string[0];

                _scannedDevices.Clear();
                _pluginObject.Call("startScanning", services);
                _onPeripheralScanned = callback;
                _scanning = true;
            }
        }

        public void StopScanning()
        {
            if (_scanning)
            {
                _pluginObject.Call("stopScanning");
                _onPeripheralScanned = null;
                _scanning = false;
            }
        }

        public void EnableBluetooth()
        {
            Debug.LogError("BluetoothLog: [Bluetooth Android] EnableBluetooth not yet implemented!");
        }

        public void DisableBluetooth()
        {
            Debug.LogError("BluetoothLog: [Bluetooth Android] DisableBluetooth not yet implemented!");
        }

        public void Connect(string mac, InternalPeripheralConnectHandler onConnect, InternalPeripheralConnectFailHandler onFail, InternalPeripheralDisconnectHandler onDisconnect)
        {
            var device = _scannedDevices.FirstOrDefault(d => d.mac == mac);

            if (device != null)
            {
                _onConnect = null;
                _onConnectFail = null;
                _onDisconnect = null;
                _onConnect = onConnect;
                _onConnectFail = onFail;
                _onDisconnect = onDisconnect;
                device.connecting = true;

                _pluginObject.Call("connect", device.device);

                _connectedDevices.Add(device);

                Debug.Log($"BluetoothLog: Should connect to {mac}");
            }
            else
            {
                Debug.LogError($"BluetoothLog: [Bluetooth Android] Unknown device {mac}");
            }
        }

        public void Disconnect(string mac)
        {
            var device = _connectedDevices.FirstOrDefault(d => d.mac == mac);

            if (device != null)
            {
                _pluginObject.Call("disconnect", device.gatt);
            }
        }

        public void GetServices(string mac, InternalServicesHandler callback)
        {
            var device = _connectedDevices.FirstOrDefault(d => d.mac == mac);

            if (device != null && device.connected)
            {
                _pluginObject.Call("getServices", device.gatt);
                _onServices = null;
                _onServices = callback;
            }
        }

        public void GetCharacteristics(string mac, string service, InternalCharacteristicHandler callback)
        {
            var device = _connectedDevices.FirstOrDefault(d => d.mac == mac);

            if (device != null && device.connected)
            {
                if (device.services.ContainsKey(service))
                {
                    _onCharacteristics = null;
                    _onCharacteristics = callback;

                    var serviceObject = device.services[service];
                    var parameters = new object[] { device.mac, serviceObject };

                    _pluginObject.Call("getCharacteristics", parameters);
                }
                else
                {
                    Debug.Log($"BluetoothLog: [Bluetooth Android] Unknown service {service}");
                }
            }
        }

        public void SetCharacteristicsUpdateCallback(string mac, InternalCharacteristicUpdateHandler callback)
        {
            var device = _connectedDevices.FirstOrDefault(d => d.mac == mac);

            if (device != null && device.connected)
            {
                _onCharacteristicUpdate = null;
                _onCharacteristicUpdate = callback;
            }
        }

        public void SubscribeToCharacteristicUpdate(string mac, string characteristic)
        {
            var device = _connectedDevices.FirstOrDefault(d => d.mac == mac);

            if (device != null && device.connected)
            {
                var characObject = device.characteristics[characteristic];
                var parameters = new object[] { device.gatt, characObject };
                _pluginObject.Call("subscribeToCharacteristicUpdate", parameters);
            }
        }

        public void WriteToCharacteristic(string mac, string characteristic, byte[] data)
        {
            var device = _connectedDevices.FirstOrDefault(d => d.mac == mac);

            if (device != null && device.connected)
            {
                var characObject = device.characteristics[characteristic];
                var parameters = new object[] { device.gatt, characObject, data };
                _pluginObject.Call("writeToCharacteristic", parameters);
            }
        }

        void OnPeripheralScanned(string name, string mac, int rssi, AndroidJavaObject device)
        {
            if (_scanning)
            {
                if (_scannedDevices.Any(s => s.mac == mac))
                {
                    var scanned = _scannedDevices.FirstOrDefault(s => s.mac == mac);
                    scanned.rssi = rssi;
                    scanned.name = name;
                }
                else
                {
                    _scannedDevices.Add(new ScannedAndroidDevice(mac, name, rssi, device));
                }

                _listener.Dispach(() => {
                    _onPeripheralScanned?.Invoke(mac, name, rssi);
                });
            }
        }

        void PeripheralConnectionStateChanged(string mac, AndroidJavaObject gatt, int status, int state)
        {
            var device = _connectedDevices.FirstOrDefault(d => d.mac == mac);

            _listener.Dispach(() =>
            {
                if (state == 2)
                {
                    if (device != null)
                    {
                        device.gatt = gatt;
                        device.connected = true;
                        device.connecting = false;
                        _onConnect?.Invoke(mac);
                    }
                    else
                        _onConnectFail?.Invoke(mac, "Unknown device connected");
                }
                else
                {
                    if (device != null)
                    {
                        if (device.connecting)
                            _onConnectFail?.Invoke(mac, "Something went wrong...");
                        else
                            _onDisconnect?.Invoke(mac, "Disconnected");

                        _connectedDevices.Remove(device);
                    }
                    else
                        _onConnectFail?.Invoke(mac, "Unknown device disconnected");
                }
            });
        }

        void OnServices(string mac, string serviceUuid, AndroidJavaObject service, int servicesSize)
        {
            var device = _connectedDevices.FirstOrDefault(d => d.mac == mac);

            if (device != null && device.connected)
            {
                if (device.mac != mac) return;

                device.services[serviceUuid] = service;

                if(device.services.Count == servicesSize)
                    _onServices?.Invoke(device.mac, device.services.Keys.ToArray());
            }
        }

        void OnCharacteristics(string mac, string serviceUuid, string characteristicUuid, AndroidJavaObject characteristic, int characteristicsSize)
        {
            var device = _connectedDevices.FirstOrDefault(d => d.mac == mac);

            if (device != null && device.connected)
            {
                if (device.mac != mac) return;

                device.characteristics[characteristicUuid] = characteristic;

                if(device.characteristics.Count == characteristicsSize)
                {
                    var service = device.services[serviceUuid];
                    var characteristics = device.characteristics.Keys.ToArray();
                    _onCharacteristics?.Invoke(mac, serviceUuid, characteristics);
                }
            }
        }

        void OnCharacteristicUpdate(string mac, string serviceUuid, string characteristicUuid, int status, string message)
        {
            var device = _connectedDevices.FirstOrDefault(d => d.mac == mac);

            if (device != null && device.connected)
            {
                if (device.mac != mac) return;

                var service = device.services[serviceUuid];
                var characteristic = device.characteristics[characteristicUuid];
                var data = Encoding.UTF8.GetBytes(message);
                _onCharacteristicUpdate?.Invoke(mac, serviceUuid, characteristicUuid, data);

            }
        }

        public class AndroidScanResultCallback : AndroidJavaProxy
        {
            internal Action<string, string, int, AndroidJavaObject> _callback;

            public AndroidScanResultCallback() : base("com.example.bleplugin.BLEScanCallback") { }

            public AndroidScanResultCallback(Action<string, string, int, AndroidJavaObject> callback) : this()
            {
                _callback = callback;
            }

            public void call(string name, string mac, int rssi, AndroidJavaObject device)
            {
                _callback?.Invoke(name, mac, rssi, device);
            }
        }

        public class AndroidConnectionChangedCallback : AndroidJavaProxy
        {
            internal Action<string, AndroidJavaObject, int, int> _callback;

            public AndroidConnectionChangedCallback() : base("com.example.bleplugin.BLEConnectionChangedCallback") { }

            public AndroidConnectionChangedCallback(Action<string, AndroidJavaObject, int, int> callback) : this()
            {
                _callback = callback;
            }

            public void call(string mac, AndroidJavaObject gatt, int status, int state)
            {
                _callback?.Invoke(mac, gatt, status, state);
            }
        }

        public class AndroidServiceCallback : AndroidJavaProxy
        {
            internal Action<string, string, AndroidJavaObject, int> _callback;

            public AndroidServiceCallback() : base("com.example.bleplugin.BLEServicesCallback") { }

            public AndroidServiceCallback(Action<string, string, AndroidJavaObject, int> callback) : this()
            {
                _callback = callback;
            }

            public void call(string mac, string serviceUuid, AndroidJavaObject service, int servicesSize)
            {
                _callback?.Invoke(mac, serviceUuid, service, servicesSize);
            }
        }

        public class AndroidCharacteristicCallback : AndroidJavaProxy
        {
            internal Action<string, string, string, AndroidJavaObject, int> _callback;

            public AndroidCharacteristicCallback() : base("com.example.bleplugin.BLEGetCharacteristicsCallback") { }

            public AndroidCharacteristicCallback(Action<string, string, string, AndroidJavaObject, int> callback) : this()
            {
                _callback = callback;
            }

            public void call(string mac, string serviceUuid, string characteristicUuid, AndroidJavaObject characteristic, int characteristicsSize)
            {
                _callback?.Invoke(mac, serviceUuid, characteristicUuid, characteristic, characteristicsSize);
            }
        }

        public class AndroidCharacteristicUpdateCallback : AndroidJavaProxy
        {
            internal Action<string, string, string, int, string> _callback;

            public AndroidCharacteristicUpdateCallback() : base("com.example.bleplugin.BLECharacteristicRead") { }

            public AndroidCharacteristicUpdateCallback(Action<string, string, string, int, string> callback) : this()
            {
                _callback = callback;
            }

            public void call(string mac, string service, string characteristic, int status, string message)
            {
                _callback?.Invoke(mac, service, characteristic, status, message);
            }
        }

        class ScannedAndroidDevice
        {
            public string mac;
            public string name;
            public int rssi;

            public bool connected;
            public bool connecting;

            public AndroidJavaObject device;
            public AndroidJavaObject gatt;
            public Dictionary<string, AndroidJavaObject> characteristics = new Dictionary<string, AndroidJavaObject>();
            public Dictionary<string, AndroidJavaObject> services = new Dictionary<string, AndroidJavaObject>();

            public ScannedAndroidDevice(string mac, string name, int rssi, AndroidJavaObject device)
            {
                this.mac = mac;
                this.name = name;
                this.rssi = rssi;
                this.device = device;
            }
        }
    }

    internal class AndroidBluetoothListener : MonoBehaviour
    {
        internal AndroidJavaObject _plugin;
        readonly Queue<Action> _actions = new Queue<Action>();

        internal void Dispach(Action action)
        {
            _actions.Enqueue(action);
        }

        void Update()
        {
            while (_actions.Count > 0)
                _actions.Dequeue()?.Invoke();
        }
    }
}

#endif