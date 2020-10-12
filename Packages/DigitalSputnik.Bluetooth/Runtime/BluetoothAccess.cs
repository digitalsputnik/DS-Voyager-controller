using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DigitalSputnik.Ble
{
    public class BluetoothAccess
    {
        private static BluetoothAccess _instance;
        #pragma warning disable 649
        private static IBluetoothInterface _interface;
        #pragma warning restore 649
        private List<PeripheralInfo> _scannedPeripherals;

        private PeripheralHandler _onPeripheralScanned;
        private PeripheralConnectionHandler _onPeripheralConnected;
        private PeripheralErrorHandler _onPeripheralConnectionFailed;
        private PeripheralErrorHandler _onPeripheralDisconnected;

        #region Public Properties
        public static IEnumerable<PeripheralInfo> ScannedPeripherals => _instance._scannedPeripherals;
        public static bool IsInitialized => _interface != null && _interface.IsInitialized();

        #endregion

        #region Public Variables
        public static void Initialize()
        {
            if (_instance == null)
            {
                _instance = new BluetoothAccess();
                SetupPlatformBluetoothInterface();
                _instance._scannedPeripherals = new List<PeripheralInfo>();
            }
            else
            {
                Debug.LogError("BluetoothLog: Bluetooth access is already initialized.");
            }
        }

        public static void StartScanning(PeripheralHandler scanned, string[] services = null)
        {
            if (IsInitialized)
                _instance.InternalStartScanning(scanned, services);
        }

        public static void StopScanning()
        {
            if (IsInitialized)
                InternalStopScanning();
        }

        public static void Connect(string id, PeripheralConnectionHandler connect, PeripheralErrorHandler fail, PeripheralErrorHandler disconnect)
        {
            if (IsInitialized)
               _instance.InternalConnect(id, connect, fail, disconnect);
        }

        public static void Disconnect(string id)
        {
            if (IsInitialized)
                InternalDisconnect(id);
        }
        #endregion

        #region Internal Methods

        private static void SetupPlatformBluetoothInterface()
        {
#if UNITY_IOS && !UNITY_EDITOR
            _interface = new IOSBluetoothInterface();
#elif UNITY_ANDROID && !UNITY_EDITOR
            _interface = new AndroidBluetoothInterface();
#endif
            _interface?.Initialize();
        }

        void InternalStartScanning(PeripheralHandler scanned, string[] services)
        {
            _onPeripheralScanned = scanned;
            _interface.StartScanning(services, PeripheralScanned);
        }

        void PeripheralScanned(string id, string name, int rssi)
        {
            var peripheral = _scannedPeripherals.FirstOrDefault(p => p.Id == id);

            if (peripheral == null)
            {
                peripheral = new PeripheralInfo(id);
                _scannedPeripherals.Add(peripheral);
            }

            peripheral.Name = name;
            peripheral.Rssi = rssi;

            _onPeripheralScanned?.Invoke(peripheral);
        }

        private static void InternalStopScanning() => _interface.StopScanning();

        private void InternalConnect(string id, PeripheralConnectionHandler connect, PeripheralErrorHandler fail, PeripheralErrorHandler disconnect)
        {
            _onPeripheralConnected = connect;
            _onPeripheralConnectionFailed = fail;
            _onPeripheralDisconnected = disconnect;
            _interface.Connect(id, OnConnect, OnConnectFail, OnDisconnect);
        }

        private void OnConnect(string id)
        {
            var access = new PeripheralAccess(id, _interface);
            _onPeripheralConnected?.Invoke(access);
        }

        private void OnConnectFail(string id, string error)
        {
            var peripheral = _scannedPeripherals.FirstOrDefault(p => p.Id == id);
            _onPeripheralConnectionFailed?.Invoke(peripheral, error);
        }

        private void OnDisconnect(string id, string error)
        {
            var peripheral = _scannedPeripherals.FirstOrDefault(p => p.Id == id);
            _onPeripheralDisconnected?.Invoke(peripheral, error);
        }

        private static void InternalDisconnect(string id) => _interface.Disconnect(id);
        #endregion
    }

    public class PeripheralInfo
    {
        public readonly string Id;
        public string Name;
        public int Rssi;

        public PeripheralInfo(string id) { Id = id; }
    }

    public delegate void PeripheralHandler(PeripheralInfo peripheral);
    public delegate void PeripheralConnectionHandler(PeripheralAccess access);
    public delegate void PeripheralErrorHandler(PeripheralInfo peripheral, string error);
    public delegate void PeripheralServicesHandler(PeripheralAccess access, string[] services);
    public delegate void PeripheralServiceCharacteristicsHandler(PeripheralAccess access, string service, string[] characteristics);
    public delegate void PeripheralCharacteristicUpdate(PeripheralAccess access, string service, string characteristic, byte[] data);

    public class PeripheralAccess
    {
        private readonly IBluetoothInterface _interface;
        private PeripheralServicesHandler _onServicesScanned;
        private PeripheralServiceCharacteristicsHandler _onServiceCharacteristicsScanned;
        private readonly Dictionary<string, Dictionary<string, PeripheralCharacteristicUpdate>> _characteristicUpdateDelegates;

        public string Id { get; }

        internal PeripheralAccess(string id, IBluetoothInterface bluetoothInterface)
        {
            Id = id;
            _interface = bluetoothInterface;
            _interface.SetCharacteristicsUpdateCallback(id, OnCharacteristicUpdate);
            _characteristicUpdateDelegates = new Dictionary<string, Dictionary<string, PeripheralCharacteristicUpdate>>();
        }

        public void GetConnectedRssi()
        {
            _interface.GetConnectedRssi(Id);
        }

        public void ScanServices(PeripheralServicesHandler scanned)
        {
            _onServicesScanned = scanned;
            _interface.GetServices(Id, OnServicesScanned);
        }

        public void ScanServiceCharacteristics(string service, PeripheralServiceCharacteristicsHandler scanned)
        {
            _onServiceCharacteristicsScanned = scanned;
            _interface.GetCharacteristics(Id, service, OnServiceCharacteristicsScanned);
        }

        public void SubscribeToCharacteristic(string service, string characteristic, PeripheralCharacteristicUpdate dataUpdate)
        {
            if (!_characteristicUpdateDelegates.ContainsKey(service))
                _characteristicUpdateDelegates.Add(service, new Dictionary<string, PeripheralCharacteristicUpdate>());

            if (!_characteristicUpdateDelegates[service].ContainsKey(characteristic))
            {
                _characteristicUpdateDelegates[service].Add(characteristic, dataUpdate);
                _interface.SubscribeToCharacteristicUpdate(Id, characteristic);
            }
        }

        public void WriteToCharacteristic(string characteristic, byte[] data)
        {
            _interface.WriteToCharacteristic(Id, characteristic, data);
        }

        private void OnServicesScanned(string id, string[] services)
        {
            if (id == Id)
            {
                _onServicesScanned?.Invoke(this, services);
            }
        }

        private void OnServiceCharacteristicsScanned(string id, string service, string[] characteristics)
        {
            if (id == Id)
            {
                _onServiceCharacteristicsScanned?.Invoke(this, service, characteristics);
            }
        }

        private void OnCharacteristicUpdate(string id, string service, string characteristic, byte[] data)
        {
            if (id != Id) return;
            if (!_characteristicUpdateDelegates.ContainsKey(service)) return;
            if (_characteristicUpdateDelegates[service].ContainsKey(characteristic))
                _characteristicUpdateDelegates[service][characteristic]?.Invoke(this, service, characteristic, data);
        }
    }
}