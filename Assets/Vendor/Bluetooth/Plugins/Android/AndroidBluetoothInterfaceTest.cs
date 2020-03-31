using System;
using System.Collections;
using System.Collections.Generic;
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

        public void Connect(object device, InternalPeripheralConnectHandlerTest onConnect, InternalPeripheralConnectFailHandlerTest onFail, InternalPeripheralDisconnectHandlerTest onDisconnect)
        {
            this.onConnect = onConnect;
            this.onDisconnect = onDisconnect;
            this.onFail = onFail;

            var onConnectionChangedCallback = new AndroidConnectionChangedCallbackTest();
            onConnectionChangedCallback._callback = OnConnectionStateChanged;

            object[] parameters = { device, onConnectionChangedCallback };

            _pluginObject.Call("connect", parameters);
        }

        public void Disconnect(object gatt)
        {
            _pluginObject.Call("disconnect", gatt);
        }

        public void GetServices(object gatt, InternalServicesHandlerTest callback)
        {
            var onServices = new AndroidServiceCallbackTest();
            onServices._callback = OnService;

            onService = callback;

            object[] parameters = { gatt, onServices };

            _pluginObject.Call("getServices", parameters);
        }

        public void GetCharacteristic(string id, object service, string uuid, InternalCharacteristicHandlerTest callback)
        {
            var getCharacteristicsCallback = new AndroidCharacteristicCallbackTest();
            getCharacteristicsCallback._callback = OnCharacteristic;

            _pluginObject.Call("setCharacteristicCallback", getCharacteristicsCallback);

            onCharacteristics = callback;

            object[] parameters = { id, service, uuid };

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

        public void SubscribeToCharacteristicUpdate(object gatt, object characteristic)
        {
            object[] parameters = { gatt, characteristic };

            _pluginObject.Call("subscribeToCharacteristicUpdate", parameters);
        }

        public void WriteToCharacteristic(object gatt, object characteristic, byte[] data)
        {
            object[] parameters = { gatt, characteristic, data };

            _pluginObject.Call("writeToCharacteristic", parameters);
        }

        void OnPeripheralScanned(string name, string mac, int rssi, AndroidJavaObject device)
        {
            onScanned?.Invoke(mac, name, rssi, device);
        }

        void OnConnectionStateChanged(string id, AndroidJavaObject gatt, int status, int newState)
        {
            if(newState == 2)
            {
                onConnect?.Invoke(id, gatt);
            }
            else
            {
                onDisconnect?.Invoke(id, "Disconnected");
            }
        }

        void OnService(string id, string serviceUuid, object service)
        {
            onService?.Invoke(id, serviceUuid, service);
        }

        void OnCharacteristic(string id, string characteristicUuid, object characteristic)
        {
            onCharacteristics?.Invoke(id, characteristicUuid, characteristic);
        }

        void OnBluetoothMessage(string id, object characteristic, int status, string message)
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
            internal Action<string, string, object> _callback;

            public AndroidServiceCallbackTest() : base("com.example.bleplugin.BLEServicesCallback") { }

            public void call(string id, string serviceUuid, object service)
            {
                _callback?.Invoke(id, serviceUuid, service);
            }
        }
        public class AndroidCharacteristicCallbackTest : AndroidJavaProxy
        {
            internal Action<string, string, object> _callback;

            public AndroidCharacteristicCallbackTest() : base("com.example.bleplugin.BLEGetCharacteristicsCallback") { }

            public void call(string id, string characteristicUuid, object characteristic)
            {
                _callback?.Invoke(id, characteristicUuid, characteristic);
            }
        }

        public class AndroidMessageCallbackTest : AndroidJavaProxy
        {
            internal Action<string, object, int, string> _callback;

            public AndroidMessageCallbackTest() : base("com.example.bleplugin.BLECharacteristicRead") { }

            public void call(string id, object characteristic, int status, string message)
            {
                _callback?.Invoke(id, characteristic, status, message);
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
