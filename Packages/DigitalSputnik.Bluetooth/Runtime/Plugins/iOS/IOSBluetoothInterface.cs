#if UNITY_IOS
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace DigitalSputnik.Ble
{
    internal class IosBluetoothInterface : IBluetoothInterface
    {
        private bool _initialized;
        private bool _scanning;

        private InternalPeripheralScanHandler _onPeripheralScanned;

        private readonly Dictionary<string, IosConnectionSession> _sessions = new Dictionary<string, IosConnectionSession>();

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
            if (_sessions.ContainsKey(id))
            {
                if (_sessions[id].Connected || _sessions[id].Connecting) return;
                
                _sessions[id].OnConnectFailed?.Invoke(id, "New connection attempt with same id");
                _sessions.Remove(id);
            }

            _sessions.Add(id, new IosConnectionSession(connect, fail, disconnect));
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
            if (_sessions.ContainsKey(id))
            {
                _sessions[id].OnServices = callback;
                _iOSGetServices(id.ToUpper());
            }
        }

        public void GetCharacteristics(string id, string service, InternalCharacteristicHandler callback)
        {
            if (_sessions.ContainsKey(id))
            {
                _sessions[id].OnCharacteristics = callback;
                _iOSGetCharacteristics(id.ToUpper(), service.ToUpper());
            }
        }

        public void SetCharacteristicsUpdateCallback(string id, InternalCharacteristicUpdateHandler callback)
        {
            if (_sessions.ContainsKey(id))
                _sessions[id].OnCharacteristicUpdate = callback;
        }

        public void SubscribeToCharacteristicUpdate(string id, string characteristic)
        {
            _iOSSubscribeToCharacteristic(id.ToUpper(), characteristic.ToUpper());
        }

        public void WriteToCharacteristic(string id, string characteristic, byte[] data)
        {
            var encoded = Convert.ToBase64String(data);
            data = Encoding.UTF8.GetBytes(encoded);
            var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            DebugConsole.LogInfo($"Writing to characteristics: {Encoding.UTF8.GetString(data)}");
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
            OnConnectingFailed(id, $"Peripheral {id} not found");
        }

        private void OnConnectingFailed(string id, string error)
        {
            if (_sessions.ContainsKey(id))
            {
                var session = _sessions[id];

                if (session.Connecting)
                {
                    session.OnConnectFailed?.Invoke(id, error);
                    session.Connecting = false;
                } 
                
                _sessions.Remove(id);
            }
        }

        private void OnConnectingSuccessful(string id)
        {
            if (_sessions.ContainsKey(id))
            {
                var session = _sessions[id];
                session.Connected = true;
                session.Connecting = false;
                session.OnConnected?.Invoke(id);
            }
        }

        private void OnDisconnect(string id, string error)
        {
            if (_sessions.ContainsKey(id))
            {
                var session = _sessions[id];

                if (session.Connecting || session.Connected)
                {
                    session.Connected = false;
                    session.Connecting = false;
                    session.OnDisconnect?.Invoke(id, error);   
                }

                _sessions.Remove(id);
            }
        }

        private void OnServices(string id, string[] services, string error)
        {
            if (_sessions.ContainsKey(id))
            {
                var session = _sessions[id];
                
                if (session.OnServices == null) return;
                
                if (string.IsNullOrEmpty(error))
                {
                    for (var i = 0; i < services.Length; i++)
                        services[i] = services[i].ToLower();
                    session.OnServices?.Invoke(id.ToLower(), services);
                }
                else
                    Debug.LogError($"[IOS Bluetooth error] {error}");

                session.OnServices = null;
            }
        }

        private void OnCharacteristics(string id, string service, string[] characteristics, string error)
        {
            if (_sessions.ContainsKey(id))
            {
                var session = _sessions[id];
                
                if (session.OnCharacteristics == null) return;
                
                if (string.IsNullOrEmpty(error))
                {
                    for (var i = 0; i < characteristics.Length; i++)
                        characteristics[i] = characteristics[i].ToLower();
                    session.OnCharacteristics?.Invoke(id.ToLower(), service.ToLower(), characteristics);
                }
                else
                    Debug.LogError($"[IOS Bluetooth error] {error}");

                session.OnCharacteristics = null;
            }
        }

        private void OnCharacteristicUpdate(string id, string service, string characteristic, string error, byte[] data)
        {
            if (_sessions.ContainsKey(id))
            {
                var session = _sessions[id];
                
                if (session.OnCharacteristicUpdate == null) return;
                
                if (string.IsNullOrEmpty(error))
                {
                    var base64 = Encoding.UTF8.GetString(data);
                    var decoded = Convert.FromBase64String(base64);
                    session.OnCharacteristicUpdate?.Invoke(id.ToLower(), service.ToLower(), characteristic.ToLower(), decoded);
                }
                else
                    Debug.LogError($"[IOS Bluetooth error] {error}");
            }
        }
#endregion
    }
}
#endif