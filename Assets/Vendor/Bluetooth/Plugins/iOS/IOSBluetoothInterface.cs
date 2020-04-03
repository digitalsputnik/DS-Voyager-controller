﻿#if UNITY_IOS && !UNITY_EDITOR
using System;
using System.Runtime.InteropServices;
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
        private static extern void _iOSStartScanning();

        [DllImport("__Internal")]
        private static extern void _iOSStopScanning();

        [DllImport("__Internal")]
        private static extern void _iOSConnect(string uid);

        [DllImport("__Internal")]
        private static extern void _iOSCancelConnection(string uid);

        [DllImport("__Internal")]
        private static extern void _iOSGetServices();

        [DllImport("__Internal")]
        private static extern void _iOSGetCharacteristics(string service);

        [DllImport("__Internal")]
        private static extern void _iOSSubscribeToCharacteristic(string service, string characteristic);

        [DllImport("__Internal")]
        private static extern void _iOSWriteToCharacteristic(string service, string characteristic, IntPtr data, int length);
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
                _iOSStartScanning();
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
            _iOSConnect(id);
        }

        public void Disconnect(string id)
        {
            _iOSCancelConnection(id);
        }

        public void GetServices(InternalServicesHandler callback)
        {
            _onServices = callback;
            _iOSGetServices();
        }

        public void GetCharacteristics(string service, InternalCharacteristicHandler callback)
        {
            _onCharacteristics = callback;
            _iOSGetCharacteristics(service);
        }

        public void SetCharacteristicsUpdateCallback(InternalCharacteristicUpdateHandler callback)
        {
            _onCharacteristicUpdate = callback;
        }

        public void SubscribeToCharacteristicUpdate(string service, string characteristic)
        {
            _iOSSubscribeToCharacteristic(service, characteristic);
        }

        public void WriterToCharacteristic(string service, string characteristic, byte[] data)
        {
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            _iOSWriteToCharacteristic(service, characteristic, handle.AddrOfPinnedObject(), data.Length);
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
                    _onCharacteristicUpdate.Invoke(peripheral, service, characteristic, data);
                else
                    Debug.LogError($"[IOS Bluetooth error] {error}");
            }
        }
#endregion
    }
}
#endif