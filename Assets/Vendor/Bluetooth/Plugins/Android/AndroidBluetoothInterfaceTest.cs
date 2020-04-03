using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Android;

namespace DigitalSputnik.Bluetooth
{
    internal class AndroidBluetoothInterfaceTest : IBluetoothInterfaceTest
    {
        const string LIBRARY_NAME = "com.example.bleplugin";

        AndroidBluetoothListenerTest _listener;
        AndroidJavaObject _pluginObject;
        AndroidJavaObject _context;

        InternalPeripheralScanHandlerTest onScanned;
        InternalPeripheralConnectHandlerTest onConnect;
        InternalPeripheralConnectFailHandlerTest onFail;
        InternalPeripheralDisconnectHandlerTest onDisconnect;
        InternalServicesHandlerTest onService;
        InternalCharacteristicHandlerTest onCharacteristics;
        InternalCharacteristicUpdateHandlerTest onMessage;

        List<AndroidBluetoothDevice> devices = new List<AndroidBluetoothDevice>();

        public void Initialize()
        {
            if (!Permission.HasUserAuthorizedPermission(Permission.CoarseLocation))
                Permission.RequestUserPermission(Permission.CoarseLocation);
            if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
                Permission.RequestUserPermission(Permission.FineLocation);

            _listener = new GameObject("Android Bluetooth Listener").AddComponent<AndroidBluetoothListenerTest>();
            _listener.StartCoroutine(AfterAuthorization());
        }

        IEnumerator AfterAuthorization()
        {
            yield return new WaitUntil(() =>
                Permission.HasUserAuthorizedPermission(Permission.CoarseLocation) &&
                Permission.HasUserAuthorizedPermission(Permission.CoarseLocation));

            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            _context = activity.Call<AndroidJavaObject>("getApplicationContext"); 

            _pluginObject = new AndroidJavaObject(LIBRARY_NAME + ".BLEObject", _context);
            _listener._plugin = _pluginObject;
        }

        public bool HasInitialized()
        {
            if (_pluginObject != null)
                return true;
            else
                return false;
        }

        public void StartScanning(string[] services, InternalPeripheralScanHandlerTest callback)
        {
            onScanned = callback;

            var scanCallback = new AndroidScanResultCallbackTest();
            scanCallback._callback = OnPeripheralScanned;

            object[] parameters = { services, scanCallback };

            _pluginObject.Call("startScanning", parameters);
        }

        public void StopScanning()
        {
            _pluginObject.Call("stopScanning");
        }

        public void EnableBluetooth()
        {
            //Not implemented
        }

        public void DisableBluetooth()
        {
            //Not implemented
        }

        public void Connect(string id, InternalPeripheralConnectHandlerTest onConnect, InternalPeripheralConnectFailHandlerTest onFail, InternalPeripheralDisconnectHandlerTest onDisconnect)
        {
            var currentDevice = devices.FirstOrDefault(l => l.id == id);

            this.onConnect = onConnect;
            this.onDisconnect = onDisconnect;
            this.onFail = onFail;

            var onConnectionChangedCallback = new AndroidConnectionChangedCallbackTest();
            onConnectionChangedCallback._callback = OnConnectionStateChanged;

            object[] parameters = { currentDevice.device, onConnectionChangedCallback };

            _pluginObject.Call("connect", parameters);
        }

        public void Disconnect(string id)
        {
            var currentDevice = devices.FirstOrDefault(l => l.id == id);

            _pluginObject.Call("disconnect", currentDevice.gatt);
        }

        public void GetServices(string id, InternalServicesHandlerTest callback)
        {
            var currentDevice = devices.FirstOrDefault(l => l.id == id);

            var onServices = new AndroidServiceCallbackTest();
            onServices._callback = OnService;

            onService = callback;

            object[] parameters = { currentDevice.gatt, onServices };

            _pluginObject.Call("getServices", parameters);
        }

        public void GetCharacteristic(string id, string service, string uuid, InternalCharacteristicHandlerTest callback)
        {
            var currentDevice = devices.FirstOrDefault(l => l.id == id);
            var currentService = currentDevice.services[service];

            var getCharacteristicsCallback = new AndroidCharacteristicCallbackTest();
            getCharacteristicsCallback._callback = OnCharacteristic;

            _pluginObject.Call("setCharacteristicCallback", getCharacteristicsCallback);

            onCharacteristics = callback;

            object[] parameters = { id, currentService, uuid };

            _pluginObject.Call("getCharacteristic", parameters);
        }

        public void SetCharacteristicsUpdateCallback(InternalCharacteristicUpdateHandlerTest callback)
        {
            var setCharacteristicsUpdateCallback = new AndroidMessageCallbackTest();
            setCharacteristicsUpdateCallback._callback = OnBluetoothMessage;

            onMessage = callback;

            object[] parameters = { setCharacteristicsUpdateCallback };

            _pluginObject.Call("setOnMessageCallback", parameters);
        }

        public void SubscribeToCharacteristicUpdate(string id, string characteristic)
        {
            var currentDevice = devices.FirstOrDefault(l => l.id == id);
            var currentCharacteristic = currentDevice.characteristics[characteristic];

            object[] parameters = { currentDevice.gatt, currentCharacteristic };

            _pluginObject.Call("subscribeToCharacteristicUpdate", parameters);
        }

        public void WriteToCharacteristic(string id, string characteristic, byte[] data)
        {
            var currentDevice = devices.FirstOrDefault(l => l.id == id);
            var currentCharacteristic = currentDevice.characteristics[characteristic];

            object[] parameters = { currentDevice.gatt, currentCharacteristic, data };

            _pluginObject.Call("writeToCharacteristic", parameters);
        }

        void OnPeripheralScanned(string name, string mac, int rssi, AndroidJavaObject device)
        {
            var newDevice = new AndroidBluetoothDevice(mac, name, rssi, device);
            devices.Add(newDevice);

            onScanned?.Invoke(mac, name, rssi);
        }

        void OnConnectionStateChanged(string id, AndroidJavaObject gatt, int status, int newState)
        {
            if(newState == 2)
            {
                var currentDevice = devices.FirstOrDefault(l => l.id == id);
                currentDevice.gatt = gatt;

                onConnect?.Invoke(id);
            }
            else
            {
                var currentDevice = devices.FirstOrDefault(l => l.id == id);
                currentDevice.gatt = null;

                onDisconnect?.Invoke(id, "Disconnected");
            }
        }

        void OnService(string id, string serviceUuid, AndroidJavaObject service)
        {
            var currentDevice = devices.FirstOrDefault(l => l.id == id);
            currentDevice.services.Add(serviceUuid, service);

            onService?.Invoke(id, serviceUuid);
        }

        void OnCharacteristic(string id, string serviceUuid, string characteristicUuid, AndroidJavaObject characteristic)
        {
            var currentDevice = devices.FirstOrDefault(l => l.id == id);
            currentDevice.characteristics.Add(characteristicUuid, characteristic);

            onCharacteristics?.Invoke(id, serviceUuid, characteristicUuid);
        }

        void OnBluetoothMessage(string id, string characteristic, int status, string message)
        {
            onMessage?.Invoke(id, characteristic, status, message);
        }

        public class AndroidScanResultCallbackTest : AndroidJavaProxy
        {
            internal Action<string, string, int, AndroidJavaObject> _callback;

            public AndroidScanResultCallbackTest() : base("com.example.bleplugin.BLEScanCallback") { }

            public void call(string name, string mac, int rssi, AndroidJavaObject device)
            {
                _callback?.Invoke(name, mac, rssi, device);
            }
        }

        public class AndroidConnectionChangedCallbackTest : AndroidJavaProxy
        {
            internal Action<string, AndroidJavaObject, int, int> _callback;

            public AndroidConnectionChangedCallbackTest() : base("com.example.bleplugin.BLEConnectionChangedCallback") { }

            public void call(string id, AndroidJavaObject gatt, int status, int newState)
            {
                _callback?.Invoke(id, gatt, status, newState);
            }
        }

        public class AndroidServiceCallbackTest : AndroidJavaProxy
        {
            internal Action<string, string, AndroidJavaObject> _callback;

            public AndroidServiceCallbackTest() : base("com.example.bleplugin.BLEServicesCallback") { }

            public void call(string id, string serviceUuid, AndroidJavaObject service)
            {
                _callback?.Invoke(id, serviceUuid, service);
            }
        }
        public class AndroidCharacteristicCallbackTest : AndroidJavaProxy
        {
            internal Action<string, string, string, AndroidJavaObject> _callback;

            public AndroidCharacteristicCallbackTest() : base("com.example.bleplugin.BLEGetCharacteristicsCallback") { }

            public void call(string id, string serviceUuid, string characteristicUuid, AndroidJavaObject characteristic)
            {
                _callback?.Invoke(id, serviceUuid, characteristicUuid, characteristic);
            }
        }

        public class AndroidMessageCallbackTest : AndroidJavaProxy
        {
            internal Action<string, string, int, string> _callback;

            public AndroidMessageCallbackTest() : base("com.example.bleplugin.BLECharacteristicRead") { }

            public void call(string id, string characteristic, int status, string message)
            {
                _callback?.Invoke(id, characteristic, status, message);
            }
        }

        class AndroidBluetoothDevice
        {
            public string id;
            public string name;
            public int rssi;

            public object device;
            public object gatt;
            public Dictionary<string, AndroidJavaObject> characteristics = new Dictionary<string, AndroidJavaObject>();
            public Dictionary<string, AndroidJavaObject> services = new Dictionary<string, AndroidJavaObject>();

            public AndroidBluetoothDevice(string _id, string _name, int _rssi, object _device)
            {
                id = _id;
                name = _name;
                rssi = _rssi;
                device = _device;
            }
        }
    }

    internal class AndroidBluetoothListenerTest : MonoBehaviour
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
