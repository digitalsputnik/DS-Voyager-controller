﻿#if UNITY_IOS
using System;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace DigitalSputnik.Ble
{
    internal class IosBluetoothInterface : IBluetoothInterface
    {
        private bool _initialized;
        private bool _scanning;

        private InternalPeripheralScanHandler _onPeripheralScanned;
        private InternalPeripheralConnectHandler _onConnected;
        private InternalPeripheralConnectFailHandler _onConnectFailed;
        private InternalPeripheralDisconnectHandler _onDisconnect;
        private InternalServicesHandler _onServices;
        private InternalCharacteristicHandler _onCharacteristics;
        private InternalCharacteristicUpdateHandler _onCharacteristicUpdate;

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
        private static extern void _iOSWriteToCharacteristic(string uid, string characteristic, IntPtr data, int length);
#endregion

#region Interface Implementation
        public bool IsInitialized()
        {
            return _initialized;
        }
        
        public void Initialize()
        {
            if (!_initialized)
            {
                var obj = new GameObject("iOS Bluetooth Listener");
                var listener = obj.AddComponent<IosBluetoothListener>();

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
                Debug.LogError("Bluetooth interface is already initialized!");
            }
        }

        public void StartScanning(string[] services, InternalPeripheralScanHandler callback)
        {
            if (!_scanning)
            {
                _onPeripheralScanned = callback;
                if (services != null)
                {
                    for (int i = 0; i < services.Length; i++)
                        services[i] = services[i].ToUpper();
                    _iOSStartScanning(services, services.Length);
                }
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

        public void Connect(string id, InternalPeripheralConnectHandler connect, InternalPeripheralConnectFailHandler fail, InternalPeripheralDisconnectHandler disconnect)
        {
            _onConnected = connect;
            _onConnectFailed = fail;
            _onDisconnect = disconnect;
            _iOSConnect(id.ToUpper());
        }

        public void GetConnectedRssi(string mac)
        {
            // TODO: Implement!
        }

        public void Reconnect(string id)
        {
            _iOSCancelConnection(id.ToUpper());
            _iOSConnect(id.ToUpper());
        }

        public void Disconnect(string id)
        {
            _iOSCancelConnection(id.ToUpper());
        }

        public void Close(string id)
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

        private void OnPeripheralScanned(string id, string name, int rssi)
        {
            _onPeripheralScanned?.Invoke(id.ToLower(), name, rssi);
        }

        private void OnPeripheralNotFound(string id)
        {
            id = id.ToLower();
            OnConnectingFailed(id, $"Peripheral {id} not found");
        }

        private void OnConnectingFailed(string id, string error)
        {
            if (_onConnectFailed == null) return;
            
            _onConnectFailed.Invoke(id.ToLower(), error);
            _onConnectFailed = null;
            _onConnected = null;
        }

        private void OnConnectingSuccessful(string id)
        {
            if (_onConnected == null) return;
            
            _onConnected.Invoke(id.ToLower());
            _onConnected = null;
            _onConnectFailed = null;
        }

        private void OnDisconnect(string id, string error)
        {
            if (_onDisconnect == null) return;
            
            _onDisconnect?.Invoke(id.ToLower(), error);
            _onDisconnect = null;
        }

        private void OnServices(string id, string[] services, string error)
        {
            if (_onServices == null) return;
            
            if (string.IsNullOrEmpty(error))
            {
                for (var i = 0; i < services.Length; i++)
                    services[i] = services[i].ToLower();
                _onServices.Invoke(id.ToLower(), services);
            }
            else
                Debug.LogError($"[IOS Bluetooth error] {error}");

            _onServices = null;
        }

        private void OnCharacteristics(string id, string service, string[] characteristics, string error)
        {
            if (_onCharacteristics == null) return;
            
            if (string.IsNullOrEmpty(error))
            {
                for (var i = 0; i < characteristics.Length; i++)
                    characteristics[i] = characteristics[i].ToLower();
                _onCharacteristics.Invoke(id.ToLower(), service.ToLower(), characteristics);
            }
            else
                Debug.LogError($"[IOS Bluetooth error] {error}");

            _onCharacteristics = null;
        }

        private void OnCharacteristicUpdate(string id, string service, string characteristic, string error, byte[] data)
        {
            if (_onCharacteristicUpdate == null) return;
            
            if (string.IsNullOrEmpty(error))
            {
                var base64 = Encoding.UTF8.GetString(data);
                var decoded = Convert.FromBase64String(base64);
                _onCharacteristicUpdate.Invoke(id.ToLower(), service.ToLower(), characteristic.ToLower(), decoded);
            }
            else
                Debug.LogError($"[IOS Bluetooth error] {error}");
        }
#endregion
    }
}
#endif