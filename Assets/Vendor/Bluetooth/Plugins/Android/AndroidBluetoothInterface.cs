#if UNITY_ANDROID && !UNITY_EDITOR

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
        const string LIBRARY_NAME = "com.digitalsputnik.dsblecito";

        AndroidJavaObject _pluginObject;
        AndroidBluetoothListener _listener;
        ScannedAndroidDevice _connectedDevice;

        InternalPeripheralScanHandler _onPeripheralScanned;
        InternalPeripheralConnectHandler _onConnect;
        InternalPeripheralConnectFailHandler _onConnectFail;
        InternalPeripheralDisconnectHandler _onDisconnect;
        InternalServicesHandler _onServices;
        InternalCharacteristicHandler _onCharacteristics;
        InternalCharacteristicUpdateHandler _onCharacteristicUpdate;

        bool _scanning;
        bool _connecting;
        bool _connected;

        List<ScannedAndroidDevice> _scannedDevices = new List<ScannedAndroidDevice>();

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

            //var scanResultsCallback = new AndroidScanResultCallback();
            //scanResultsCallback._callback = OnPeripheralScanned;
            //_pluginObject.Call("setOnScanResultCallback", scanResultsCallback);

            _pluginObject = new AndroidJavaObject(LIBRARY_NAME + ".BLEObject", context);
            _listener._plugin = _pluginObject;
        }

        public void StartScanning(string[] services, InternalPeripheralScanHandler callback)
        {
            if (!_scanning)
            {   
                if (services == null)
                    services = new string[0];

                var scanCallback = new AndroidScanResultCallback(OnPeripheralScanned);
                var parameters = new object[] { services, scanCallback };

                _scannedDevices.Clear();
                _pluginObject.Call("startScanning", parameters);
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
                _onConnect = onConnect;
                _onConnectFail = onFail;
                _onDisconnect = onDisconnect;
                _connecting = true;

                var onConnectionChanged = new AndroidConnectionChangedCallback(PeripheralConnectionStateChanged);
                var parameters = new object[] { device.device, onConnectionChanged };

                _pluginObject.Call("connect", parameters);

                _connectedDevice = device;

                Debug.Log($"BluetoothLog: should connect to {mac}");
            }
            else
            {
                Debug.LogError($"BluetoothLog: [Bluetooth Android] Unknown device {mac}");
            }
        }

        public void Disconnect(string mac)
        {
            if (_connectedDevice != null)
            {
                _pluginObject.Call("disconnect", _connectedDevice.gatt);
            }
        }

        public void GetServices(InternalServicesHandler callback)
        {
            if (_connectedDevice != null && _connected)
            {
                var onServices = new AndroidServiceCallback(OnServices);
                var parameters = new object[] { _connectedDevice.gatt, onServices };

                _pluginObject.Call("getServices", parameters);
                _onServices = callback;
            }
        }

        public void GetCharacteristics(string service, InternalCharacteristicHandler callback)
        {
            if (_connectedDevice != null && _connected)
            {
                if (_connectedDevice.services.ContainsKey(service))
                {
                    _onCharacteristics = callback;

                    var serviceObject = _connectedDevice.services[service];
                    var onCharacteristics = new AndroidCharacteristicCallback(OnCharacteristics);
                    var parameters = new object[] { _connectedDevice.mac, serviceObject, service };

                    _pluginObject.Call("setCharacteristicCallback", onCharacteristics);
                    _pluginObject.Call("getCharacteristic", parameters);
                }
                else
                {
                    Debug.LogError($"BluetoothLog: [Bluetooth Android] Unknown service {service}");
                }
            }
        }

        public void SetCharacteristicsUpdateCallback(InternalCharacteristicUpdateHandler callback)
        {
            if (_connectedDevice != null && _connected)
            {
                _onCharacteristicUpdate = callback;
                var onCharacteristicsUpdate = new AndroidCharacteristicUpdateCallback(OnCharacteristicUpdate);
                _pluginObject.Call("setOnMessageCallback", onCharacteristicsUpdate);
            }
        }

        public void SubscribeToCharacteristicUpdate(string service, string characteristic)
        {
            if (_connectedDevice != null && _connected)
            {
                var characObject = _connectedDevice.characteristics[characteristic];
                var parameters = new object[] { _connectedDevice.gatt, characObject };
                _pluginObject.Call("subscribeToCharacteristicUpdate", parameters);
            }
        }

        public void WriterToCharacteristic(string service, string characteristic, byte[] data)
        {
            if (_connectedDevice != null && _connected)
            {
                var characObject = _connectedDevice.characteristics[characteristic];
                var parameters = new object[] { _connectedDevice.gatt, characObject, data };
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
            _listener.Dispach(() =>
            {
                if (state == 2)
                {
                    if (_connectedDevice != null)
                    {
                        _connectedDevice.gatt = gatt;
                        _onConnect?.Invoke(mac);
                        _connected = true;
                    }
                    else
                        _onConnectFail?.Invoke(mac, "Unknown device connected");
                }
                else
                {
                    if (_connectedDevice != null)
                    {
                        _connectedDevice.gatt = null;
                        _connectedDevice = null;
                        _connected = false;

                        if (_connecting)
                            _onConnectFail?.Invoke(mac, "Something went wrong...");
                        else
                            _onDisconnect?.Invoke(mac, "Disconnected");
                    }
                    else
                        _onConnectFail?.Invoke(mac, "Unknown device disconnected");
                }

                _connecting = false;
            });
        }

        void OnServices(string mac, string serviceUuid, AndroidJavaObject service)
        {
            if(_connectedDevice != null && _connected)
            {
                if (_connectedDevice.mac != mac) return;

                _connectedDevice.services[serviceUuid] = service;
                var services = _connectedDevice.services.Keys.ToArray();
                _onServices?.Invoke(_connectedDevice.mac, services);
            }
        }

        // TODO: Get correct service
        void OnCharacteristics(string mac, string characteristicUuid, AndroidJavaObject characteristic)
        {
            if (_connectedDevice != null && _connected)
            {
                if (_connectedDevice.mac != mac) return;

                _connectedDevice.characteristics[characteristicUuid] = characteristic;

                //var service = _connectedDevice.services[""];
                var characteristics = _connectedDevice.characteristics.Keys.ToArray();
                _onCharacteristics?.Invoke(mac, "", characteristics);
            }
        }

        // TODO: Get service & characteristic with message
        void OnCharacteristicUpdate(string mac, int status, string message)
        {
            if (_connectedDevice != null && _connected)
            {
                if (_connectedDevice.mac != mac) return;

                var service = "?";
                var characteristic = "?";
                var data = Encoding.UTF8.GetBytes(message);
                _onCharacteristicUpdate?.Invoke(mac, service, characteristic, data);
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
            internal Action<string, string, AndroidJavaObject> _callback;

            public AndroidServiceCallback() : base("com.example.bleplugin.BLEServicesCallback") { }

            public AndroidServiceCallback(Action<string, string, AndroidJavaObject> callback) : this()
            {
                _callback = callback;
            }

            public void call(string mac, string serviceUuid, AndroidJavaObject service)
            {
                _callback?.Invoke(mac, serviceUuid, service);
            }
        }

        public class AndroidCharacteristicCallback : AndroidJavaProxy
        {
            internal Action<string, string, AndroidJavaObject> _callback;

            public AndroidCharacteristicCallback() : base("com.example.bleplugin.BLEGetCharacteristicsCallback") { }

            public AndroidCharacteristicCallback(Action<string, string, AndroidJavaObject> callback) : this()
            {
                _callback = callback;
            }

            public void call(string mac, string characteristicUuid, AndroidJavaObject characteristic)
            {
                _callback?.Invoke(mac, characteristicUuid, characteristic);
            }
        }

        public class AndroidCharacteristicUpdateCallback : AndroidJavaProxy
        {
            internal Action<string, int, string> _callback;

            public AndroidCharacteristicUpdateCallback() : base("com.example.bleplugin.BLECharacteristicRead") { }

            public AndroidCharacteristicUpdateCallback(Action<string, int, string> callback) : this()
            {
                _callback = callback;
            }

            public void call(string mac, int status, string message)
            {
                _callback?.Invoke(mac, status, message);
            }
        }

        class ScannedAndroidDevice
        {
            public string mac;
            public string name;
            public int rssi;

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