#if UNITY_IOS
using System;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace DigitalSputnik.Bluetooth
{
    internal class IOSBluetoothInterface : IBluetoothInterface
    {
        bool _initialized;
        bool _scanning;

        InternalPeripheralScanHandler _onPeripheralScanned;

        InternalPeripheralConnectHandler _onConnected;
        InternalPeripheralConnectFailHandler _onConnectFailed;
        InternalPeripheralDisconnectHandler _onDisconnect;
        InternalServicesHandler _onServices;
        InternalCharacteristicHandler _onCharacteristics;
        InternalCharacteristicUpdateHandler _onCharacteristicUpdate;

#region Bindings
        [DllImport("__Internal")]
        private static extern void _iOSInitialize();

        [DllImport("__Internal")]
        private static extern void _iOSStartScanning(string[] services, int count);

        [DllImport("__Internal")]
        private static extern void _iOSStopScanning();

        [DllImport("__Internal")]
        private static extern void _iOSConnect(string uid);

        [DllImport("__Internal")]
        private static extern void _iOSCancelConnection(string uid);

        [DllImport("__Internal")]
        private static extern void _iOSGetServices(string uid);

        [DllImport("__Internal")]
        private static extern void _iOSGetCharacteristics(string uid, string service);

        [DllImport("__Internal")]
        private static extern void _iOSSubscribeToCharacteristic(string uid, string characteristic);

        [DllImport("__Internal")]
        private static extern void _iOSWriteToCharacteristic(string uid, string characteristic, IntPtr data, int length);
#endregion

#region Interface Implementation
        public void Initialize()
        {
            if (!_initialized)
            {
                GameObject obj = new GameObject("iOS Bluetooth Listener");
                IOSBluetoothListener listener = obj.AddComponent<IOSBluetoothListener>();

                listener.OnPeripheralScanned += OnPeripheralScanned;
                listener.OnPeripheralNotFound += OnPeripheralNotFound;
                listener.OnConnectingSuccessful += OnConnectingSuccessful;
                listener.OnConnectingFailed += OnConnectingFailed;
                listener.OnDisconnect += OnDisconnect;
                listener.OnServices += OnServices;
                listener.OnCharacteristics += OnCharacteristics;
                listener.OnCharacteristicUpdate += OnCharacteristicUpdate;

                _iOSInitialize();

                _initialized = true;
            }
            else
            {
                Debug.LogError("Bluetooth interface is allready initialized!");
            }
        }

        public void StartScanning(string[] services, InternalPeripheralScanHandler callback)
        {
            if (!_scanning)
            {
                _onPeripheralScanned = callback;
                if (services != null)
                    _iOSStartScanning(services, services.Length);
                else
                    _iOSStartScanning(null, 0);
                _scanning = true;
            }
            else
            {
                Debug.LogError("Already scanning bluetooth devices!");
            }
        }

        public void StopScanning()
        {
            if (_scanning)
            {
                _iOSStopScanning();
                _onPeripheralScanned = null;
                _scanning = false;
            }
        }

        public void Connect(string id, InternalPeripheralConnectHandler onConnect, InternalPeripheralConnectFailHandler onFail, InternalPeripheralDisconnectHandler onDisconnect)
        {
            _onConnected = onConnect;
            _onConnectFailed = onFail;
            _onDisconnect = onDisconnect;
            _iOSConnect(id.ToUpper());
        }

        public void Disconnect(string id)
        {
            _iOSCancelConnection(id.ToUpper());
        }

        public void GetServices(string id, InternalServicesHandler callback)
        {
            _onServices = callback;
            _iOSGetServices(id.ToUpper());
        }

        public void GetCharacteristics(string id, string service, InternalCharacteristicHandler callback)
        {
            _onCharacteristics = callback;
            _iOSGetCharacteristics(id.ToUpper(), service.ToUpper());
        }

        public void SetCharacteristicsUpdateCallback(string id, InternalCharacteristicUpdateHandler callback)
        {
            _onCharacteristicUpdate = callback;
        }

        public void SubscribeToCharacteristicUpdate(string id, string characteristic)
        {
            _iOSSubscribeToCharacteristic(id.ToUpper(), characteristic.ToUpper());
        }

        public void WriteToCharacteristic(string id, string characteristic, byte[] data)
        {
            string encoded = Convert.ToBase64String(data);
            data = Encoding.UTF8.GetBytes(encoded);

            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            _iOSWriteToCharacteristic(id.ToUpper(), characteristic.ToUpper(), handle.AddrOfPinnedObject(), data.Length);
            handle.Free();
        }

        // On iOS bluetooth is force enabled when app starts.
        public void EnableBluetooth() { }

        // On iOS bluetooth cannot be disabled if allready initialized.
        public void DisableBluetooth() { }
#endregion

#region Event Handling
        void OnPeripheralScanned(string id, string name, int rssi)
        {
            _onPeripheralScanned?.Invoke(id, name, rssi);
        }

        void OnPeripheralNotFound(string peripheral)
        {
            OnConnectingFailed(peripheral, $"Peripheral {peripheral} not found");
        }

        void OnConnectingFailed(string peripheral, string error)
        {
            if (_onConnectFailed != null)
            {
                _onConnectFailed.Invoke(peripheral, error);
                _onConnectFailed = null;
                _onConnected = null;
            }
        }

        void OnConnectingSuccessful(string peripheral)
        {
            if (_onConnected != null)
            {
                _onConnected.Invoke(peripheral);
                _onConnected = null;
                _onConnectFailed = null;
            }
        }

        void OnDisconnect(string peripheral, string error)
        {
            if (_onDisconnect != null)
            {
                _onDisconnect?.Invoke(peripheral, error);
                _onDisconnect = null;
            }
        }

        void OnServices(string peripheral, string[] services, string error)
        {
            if (_onServices != null)
            {
                if (string.IsNullOrEmpty(error))
                    _onServices.Invoke(peripheral, services);
                else
                    Debug.LogError($"[IOS Bluetooth error] {error}");

                _onServices = null;
            }
        }

        void OnCharacteristics(string peripheral, string service, string[] characteristics, string error)
        {
            if (_onCharacteristics != null)
            {
                if (string.IsNullOrEmpty(error))
                    _onCharacteristics.Invoke(peripheral, service, characteristics);
                else
                    Debug.LogError($"[IOS Bluetooth error] {error}");

                _onCharacteristics = null;
            }
        }

        void OnCharacteristicUpdate(string peripheral, string service, string characteristic, string error, byte[] data)
        {

            if (_onCharacteristicUpdate != null)
            {
                if (string.IsNullOrEmpty(error))
                {
                    var base64 = Encoding.UTF8.GetString(data);
                    var decoded = Convert.FromBase64String(base64);
                    _onCharacteristicUpdate.Invoke(peripheral, service, characteristic, decoded);
                }
                else
                    Debug.LogError($"[IOS Bluetooth error] {error}");
            }
        }
#endregion
    }
}
#endif