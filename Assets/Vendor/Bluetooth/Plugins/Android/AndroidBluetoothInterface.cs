using System;
using System.Collections;
using System.Collections.Generic;
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
        AndroidJavaObject _connectedDevice;

        InternalPeripheralScanHandler _onPeripheralScanned;
        InternalPeripheralConnectHandler _onConnect;
        InternalPeripheralConnectFailHandler _onConnectFail;
        InternalPeripheralDisconnectHandler _onDisconnect;
        InternalCharacteristicUpdateHandler _onCharacteristicUpdate;

        bool _scanning;
        string _attemptingToConnectMac;
        bool _connecting;

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

            var scanResultsCallback = new AndroidScanResultCallback();
            scanResultsCallback._callback = OnPeripheralScanned;

            _pluginObject = new AndroidJavaObject(LIBRARY_NAME + ".BLEScanner", context);
            _pluginObject.Call("setOnScanResultCallback", scanResultsCallback);
            _listener._plugin = _pluginObject;
        }

        public void StartScanning(string[] services, InternalPeripheralScanHandler callback)
        {
            if (!_scanning)
            {   
                if (services == null)
                    services = new string[0];

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

        public void Connect(string id, InternalPeripheralConnectHandler onConnect, InternalPeripheralConnectFailHandler onFail, InternalPeripheralDisconnectHandler onDisconnect)
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject context = activity.Call<AndroidJavaObject>("getApplicationContext");

            _onConnect = onConnect;
            _onConnectFail = onFail;
            _onDisconnect = onDisconnect;

            _connecting = true;
            _attemptingToConnectMac = id;

            foreach (var lamp in BluetoothTest.instance.bleItems)
            {
                if (lamp.id == id.ToString())
                {
                    lamp.androidDevice = _pluginObject.Call<AndroidJavaObject>("getDevice", context, id);
                    var onConnectionChanged = new AndroidConnectionChangedCallback();
                    _connectedDevice = lamp.androidDevice;
                    onConnectionChanged._callback = PeripheralConnectionStateChanged;
                    lamp.androidDevice.Call("setOnConnectChanged", onConnectionChanged);
                    lamp.androidDevice.Call("startConnecting");

                    Debug.Log($"BluetoothLog: should connect to {id}");
                }
            }
        }

        public void Reconnect(string id)
        {
            BluetoothTest.UpdateInfoText($"Failed. Reconnecting.. ");

            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject context = activity.Call<AndroidJavaObject>("getApplicationContext");

            foreach (var lamp in BluetoothTest.instance.bleItems)
            {
                if (lamp.id == id.ToString())
                {
                    _connecting = true;
                    lamp.androidDevice = _pluginObject.Call<AndroidJavaObject>("getDevice", context, id);
                    var onConnectionChanged = new AndroidConnectionChangedCallback();
                    _connectedDevice = lamp.androidDevice;
                    onConnectionChanged._callback = PeripheralConnectionStateChanged;
                    lamp.androidDevice.Call("setOnConnectChanged", onConnectionChanged);
                    lamp.androidDevice.Call("startConnecting");

                    Debug.Log($"BluetoothLog: should connect to {id}");
                }
            }
        }

        public void Disconnect(string id)
        {
            foreach(var lamp in BluetoothTest.instance.bleItems)
            {
                if(lamp.androidDevice != null)
                {
                    if (_connecting)
                        lamp.androidDevice.Call("stopConnecting");
                    else
                        lamp.androidDevice.Call("disconnect");
                }
            }
        }

        public void GetServices(InternalServicesHandler callback)
        {
            Debug.LogError("BluetoothLog: [Bluetooth Android] GetServices not yet implemented!");
        }

        public void GetCharacteristics(string service, InternalCharacteristicHandler callback)
        {
            Debug.LogError("BluetoothLog: [Bluetooth Android] GetCharacteristics not yet implemented!");
        }

        public void SetCharacteristicsUpdateCallback(InternalCharacteristicUpdateHandler callback)
        {
            _onCharacteristicUpdate = callback;
        }

        public void SubscribeToCharacteristicUpdate(string service, string characteristic)
        {

        }

        public void WriterToCharacteristic(string service, string characteristic, byte[] data)
        {
            var sData = Array.ConvertAll(data, b => unchecked((sbyte)b));
            _connectedDevice.Call("write", sData);
        }

        void OnPeripheralScanned(string name, string mac, int rssi)
        {
            if (_scanning)
            {
                _listener.Dispach(() => {
                    _onPeripheralScanned?.Invoke(mac, name, rssi);
                });
            }
        }

        void PeripheralConnectionStateChanged(bool connected)
        {
            _listener.Dispach(() => {
                Debug.Log($"BluetoothLog: Connection state changed on {_attemptingToConnectMac} to {connected}");

                if (connected)
                {
                    if (_onConnect != null)
                    {
                        _onConnect.Invoke(_attemptingToConnectMac);
                        _onConnect = null;

                        var messageCallback = new AndroidMessageCallback();
                        messageCallback._callback = OnBluetoothMessage;
                        _connectedDevice.Call("setOnMessageCallback", messageCallback); 
                    }

                    _connecting = false;
                }
                else
                {
                    if (_onDisconnect != null)
                    {
                        if (!_connecting)
                        {
                            _onDisconnect.Invoke(_attemptingToConnectMac, "");
                            _onDisconnect = null;
                            _connecting = false;
                        }
                        else
                        {
                            Disconnect(_attemptingToConnectMac);
                            Reconnect(_attemptingToConnectMac);
                        }
                    }
                    else
                        _connecting = false;
                }
            });
        }

        void OnBluetoothMessage(string message)
        {
            Debug.Log(message);
            _onCharacteristicUpdate?.Invoke(_attemptingToConnectMac, "", "", Encoding.UTF8.GetBytes(message));
        }

        public class AndroidScanResultCallback : AndroidJavaProxy
        {
            internal Action<string, string, int> _callback;

            public AndroidScanResultCallback() : base("com.digitalsputnik.dsblecito.IBLEScanCallback") { }

            public void call(string name, string mac, int dbm)
            {
                _callback?.Invoke(name, mac, dbm);
            }
        }

        public class AndroidConnectionChangedCallback : AndroidJavaProxy
        {
            internal Action<bool> _callback;

            public AndroidConnectionChangedCallback() : base("com.digitalsputnik.dsblecito.IConnectChangeCallback") { }

            public void call(bool connected)
            {
                _callback?.Invoke(connected);
            }
        }

        public class AndroidMessageCallback : AndroidJavaProxy
        {
            internal Action<string> _callback;

            public AndroidMessageCallback() : base("com.digitalsputnik.dsblecito.IMessageCallback") { }

            public void call(string message)
            {
                _callback?.Invoke(message);
            }
        }
    }

    internal class AndroidBluetoothListener : MonoBehaviour
    {
        internal AndroidJavaObject _plugin;
        Queue<Action> actions = new Queue<Action>();

        internal void Dispach(Action action)
        {
            actions.Enqueue(action);
        }

        void Update()
        {
            while (actions.Count > 0)
            {
                actions.Dequeue()?.Invoke();
            }
        }
    }
}
