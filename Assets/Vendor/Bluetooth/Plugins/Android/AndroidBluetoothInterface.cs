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
        AndroidJavaObject _connectedDevice;

        InternalPeripheralScanHandler _onPeripheralScanned;
        InternalPeripheralConnectHandler _onConnect;
        InternalPeripheralConnectFailHandler _onConnectFail;
        InternalPeripheralDisconnectHandler _onDisconnect;
        InternalCharacteristicUpdateHandler _onCharacteristicUpdate;

        bool _scanning;
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

            if (_onConnect == null || _onConnectFail == null || _onDisconnect == null)
            {
                _onConnect = onConnect;
                _onConnectFail = onFail;
                _onDisconnect = onDisconnect;
            }

            _connecting = true;

            var lamp = BluetoothTest.instance.bleItems.FirstOrDefault(l => l.id == id);
            var onConnectionChanged = new AndroidConnectionChangedCallback();
            onConnectionChanged._callback = PeripheralConnectionStateChanged;

            lamp.androidDevice = _pluginObject.Call<AndroidJavaObject>("getDevice", context, id);
            lamp.androidDevice.Call("setOnConnectChanged", onConnectionChanged);
            lamp.androidDevice.Call("startConnecting");

            _connectedDevice = lamp.androidDevice;

            Debug.Log($"BluetoothLog: should connect to {id}");
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

        void PeripheralConnectionStateChanged(bool connected, AndroidJavaObject device)
        {
            _listener.Dispach(() => {

                string mac = device.Call<string>("getMac");
                Debug.Log($"BluetoothLog: Connection state changed on {mac} to {connected}");

                if (connected)
                {
                    if (_onConnect != null)
                    {
                        _onConnect.Invoke(mac);
                        _onConnect = null;

                        var messageCallback = new AndroidMessageCallback();
                        messageCallback._callback = OnBluetoothMessage;
                        device.Call("setOnMessageCallback", messageCallback);
                    }

                    _connecting = false;
                }
                else
                {
                    if (_onDisconnect != null)
                    {
                        if (!_connecting)
                        {
                            _onDisconnect.Invoke(mac, "");
                            _onDisconnect = null;
                            _connecting = false;
                        }
                        else
                        {
                            //Disconnect(mac);
                            Connect(mac, null, null, null);
                        }
                    }
                    else
                        _connecting = false;
                }
            });
        }

        void OnBluetoothMessage(string message, AndroidJavaObject device)
        {
            Debug.Log(message);
            _onCharacteristicUpdate?.Invoke(device.Call<string>("getMac"), "", "", Encoding.UTF8.GetBytes(message));
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
            internal Action<bool, AndroidJavaObject> _callback;

            public AndroidConnectionChangedCallback() : base("com.digitalsputnik.dsblecito.IConnectChangeCallback") { }

            public void call(bool connected, AndroidJavaObject device)
            {
                _callback?.Invoke(connected, device);
            }
        }

        public class AndroidMessageCallback : AndroidJavaProxy
        {
            internal Action<string, AndroidJavaObject> _callback;

            public AndroidMessageCallback() : base("com.digitalsputnik.dsblecito.IMessageCallback") { }

            public void call(string message, AndroidJavaObject device)
            {
                _callback?.Invoke(message, device);
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
