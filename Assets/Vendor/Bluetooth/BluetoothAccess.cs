// -----------------------------------------------------------------
// Author: Taavet Maask	Date: 11/20/2019
// Copyright: © Digital Sputnik OÜ
// -----------------------------------------------------------------

/*using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VoyagerApp.UI.Menus;

namespace DigitalSputnik.Bluetooth
{
    public class BluetoothAccess
    {
        static BluetoothAccess _instance;

        IBluetoothInterface _interface;
        List<PeripheralInfo> _scannedPeripherals;

        PeripheralHandler _onPeripheralScanned;
        PeripheralConnectionHandler _onPeripheralConnected;
        PeripheralErrorHandler _onPeripheralConnectionFailed;
        PeripheralErrorHandler _onPeripheralDisconnected;

        #region Public Properties
        public static List<PeripheralInfo> ScannedPeripherals => _instance._scannedPeripherals;

        public static bool IsInitialized
        {
            get
            {
                if (_instance != null)
                {
                    return true;
                }

                Debug.LogError("Bluetooth access is not initialized.");
                return false;
            }
        }
        #endregion

        #region Public Variables
        public static void Initialize()
        {
            if (_instance == null)
            {
                _instance = new BluetoothAccess();
                _instance.SetupPlatformBluetoothInterface();
                _instance._scannedPeripherals = new List<PeripheralInfo>();
            }
            else
            {
                Debug.LogError("BluetoothLog: Bluetooth access is allready initialized.");
            }
        }

        public static void StartScanning(PeripheralHandler onScanned, string[] services = null)
        {
            if (IsInitialized)
                _instance.InternalStartScanning(onScanned, services);
        }

        public static void StopScanning()
        {
            if (IsInitialized)
                _instance.InternalStopScanning();
        }

        public static void Connect(string peripheral, PeripheralConnectionHandler onConnect, PeripheralErrorHandler onFail, PeripheralErrorHandler onDisconnect)
        {
            if (IsInitialized)
               _instance.InternalConnect(peripheral, onConnect, onFail, onDisconnect);
        }

        public static void Disconnect(string peripheral)
        {
            if (IsInitialized)
                _instance.InternalDisconnect(peripheral);
        }
        #endregion

        #region Internal Methods
        void SetupPlatformBluetoothInterface()
        {
#if UNITY_IOS && !UNITY_EDITOR
            _interface = new IOSBluetoothInterface();
#elif UNITY_ANDROID && !UNITY_EDITOR
            _interface = new AndroidBluetoothInterface();
#endif
            _interface.Initialize();
        }

        void InternalStartScanning(PeripheralHandler onScanned, string[] services)
        {
            _onPeripheralScanned = onScanned;
            _interface.StartScanning(services, PeripheralScanned);
        }

        void PeripheralScanned(string id, string name, int rssi)
        {
            var peripheral = _scannedPeripherals.FirstOrDefault(p => p.id == id);

            if (peripheral == null)
            {
                peripheral = new PeripheralInfo(id);
                _scannedPeripherals.Add(peripheral);
            }

            peripheral.name = name;
            peripheral.rssi = rssi;

            _onPeripheralScanned?.Invoke(peripheral);
        }

        void InternalStopScanning()
        {
            _interface.StopScanning();
        }

        void InternalConnect(string id, PeripheralConnectionHandler onConnect, PeripheralErrorHandler onFail, PeripheralErrorHandler onDisconnect)
        {
            _onPeripheralConnected = onConnect;
            _onPeripheralConnectionFailed = onFail;
            _onPeripheralDisconnected = onDisconnect;
            _interface.Connect(id, OnConnect, OnConnectFail, OnDisconnect);
        }

        private void OnConnect(string id)
        {
            BLEItem bleItem = GetBleItemById(id);
            PeripheralAccess access = new PeripheralAccess(id, _interface);
            _onPeripheralConnected?.Invoke(access);
            bleItem.peripheralAccess = access;
            bleItem.connected = true;
        }

        private void OnConnectFail(string id, string error)
        {
            BLEItem bleItem = GetBleItemById(id);
            bleItem.connected = false;
            _onPeripheralConnectionFailed?.Invoke(bleItem.peripheral, error);
        }

        private void OnDisconnect(string id, string error)
        {
            BLEItem bleItem = GetBleItemById(id);
            bleItem.connected = false;
            _onPeripheralDisconnected?.Invoke(bleItem.peripheral, error);
        }

        private BLEItem GetBleItemById(string id)
        {
            return null; //BluetoothTest.instance.bleItems.FirstOrDefault(l => l.id == id) as BLEItem;
        }

        void InternalDisconnect(string id)
        {
            _interface.Disconnect(id);
        }
        #endregion
    }

    public class PeripheralInfo
    {
        public string id;
        public string name;
        public int rssi;

        public PeripheralInfo(string id)
        {
            this.id = id;
        }
    }

    public delegate void PeripheralHandler(PeripheralInfo peripheral);
    public delegate void PeripheralConnectionHandler(PeripheralAccess access);
    public delegate void PeripheralErrorHandler(PeripheralInfo peripheral, string error);
    public delegate void PeripheralServicesHandler(PeripheralAccess access, string[] services);
    public delegate void PeripheralServiceCharacteristicsHandler(PeripheralAccess access, string service, string[] characteristics);
    public delegate void PeripheralCharacteristicUpdate(PeripheralAccess access, string service, string characteristic, byte[] data);

    public class PeripheralAccess
    {
        IBluetoothInterface _interface;
        string _id;

        PeripheralServicesHandler _onServicesScanned;
        PeripheralServiceCharacteristicsHandler _onServiceCharacteristicsScanned;
        Dictionary<string, Dictionary<string, PeripheralCharacteristicUpdate>> _characteristicUpdateDelegates;

        public string ID => _id;

        internal PeripheralAccess(string id, IBluetoothInterface bluetoothInterface)
        {
            _id = id;
            _interface = bluetoothInterface;
            _interface.SetCharacteristicsUpdateCallback(OnCharacteristicUpdate);
            _characteristicUpdateDelegates = new Dictionary<string, Dictionary<string, PeripheralCharacteristicUpdate>>();
        }

        public void ScanServices(PeripheralServicesHandler onScanned)
        {
            _onServicesScanned = onScanned;
            _interface.GetServices(OnServicesScanned);
        }

        public void ScanServiceCharacteristics(string service, PeripheralServiceCharacteristicsHandler onScanned)
        {
            _onServiceCharacteristicsScanned = onScanned;
            _interface.GetCharacteristics(service, OnServiceCharacteristicsScanned);
        }

        public void SubscrubeToCharacteristic(string service, string characteristic, PeripheralCharacteristicUpdate onDataUpdate)
        {
            if (!_characteristicUpdateDelegates.ContainsKey(service))
                _characteristicUpdateDelegates.Add(service, new Dictionary<string, PeripheralCharacteristicUpdate>());

            if (!_characteristicUpdateDelegates[service].ContainsKey(characteristic))
            {
                _characteristicUpdateDelegates[service].Add(characteristic, onDataUpdate);
                _interface.SubscribeToCharacteristicUpdate(service, characteristic);
            }
        }

        public void WriteToCharacteristic(string service, string characteristic, byte[] data)
        {
            _interface.WriterToCharacteristic(service, characteristic, data);
        }

        void OnServicesScanned(string id, string[] services)
        {
            if (id == _id)
            {
                _onServicesScanned?.Invoke(this, services);
            }
        }

        void OnServiceCharacteristicsScanned(string id, string service, string[] characteristics)
        {
            if (id == _id)
            {
                _onServiceCharacteristicsScanned?.Invoke(this, service, characteristics);
            }
        }

        void OnCharacteristicUpdate(string id, string service, string characteristic, byte[] data)
        {
            if (id == _id)
            {
                if (_characteristicUpdateDelegates.ContainsKey(service))
                {
                    if (_characteristicUpdateDelegates[service].ContainsKey(characteristic))
                    {
                        _characteristicUpdateDelegates[service][characteristic]?.Invoke(this, service, characteristic, data);
                    }
                }
            }
        }
    }
}*/