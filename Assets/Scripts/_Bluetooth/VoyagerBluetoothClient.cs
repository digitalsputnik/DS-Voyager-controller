using System.Collections.Generic;
using System.Linq;
using System.Text;
using DigitalSputnik;
using DigitalSputnik.Ble;
using DigitalSputnik.Colors;
using DigitalSputnik.Voyager;
using DigitalSputnik.Voyager.Communication;

namespace VoyagerController
{
    public class VoyagerBluetoothClient : VoyagerClient
    {
        private static VoyagerBluetoothClient _instance;
        
        private const double INITIALIZATION_TIME = 0.5;
        private const double SCAN_RESTART_TIME = 30.0;
        private const string SERVICE_UID = "6e400001-b5a3-f393-e0a9-e50e24dcca9e";
        private const string UART_RX_CHARACTERISTIC_UUID = "6e400002-b5a3-f393-e0a9-e50e24dcca9e";
        private const string UART_TX_CHARACTERISTIC_UUID = "6e400003-b5a3-f393-e0a9-e50e24dcca9e";
        private const int MAX_CONNECTIONS = 4;
        private const double LEFT_OUT_TIME = 60.0;

        private ClientState _state = ClientState.WaitingForInitialization;
        private double _initializedTime = 0.0;
        private double _lastScanStarted = 0.0;
        
        private readonly List<BluetoothConnection> _connections = new List<BluetoothConnection>();
        private readonly List<string> _inActiveConnections = new List<string>();

        public static void SendMessage(VoyagerLamp lamp, byte[] message)
        {
            // TODO: Implement
        }
        
        public VoyagerBluetoothClient()
        {
            BluetoothAccess.Initialize();
            _instance = this;
        }
    
        protected override void Update()
        {
            switch (_state)
            {
                case ClientState.WaitingForInitialization when BluetoothAccess.IsInitialized:
                    OnBluetoothInitializedByDevice();
                    break;
                case ClientState.Initialized when TimeUtils.Epoch > _initializedTime + INITIALIZATION_TIME:
                    OnBluetoothWaitedByController();
                    break;
                case ClientState.Ready:
                    CheckToRestartScanning();
                    break;
            }

            base.Update();
        }

        private void OnBluetoothInitializedByDevice()
        {
            _state = ClientState.Initialized;
            _initializedTime = TimeUtils.Epoch;
            Debugger.LogInfo("Bluetooth is initialized by device");
        }

        private void OnBluetoothWaitedByController()
        {
            _state = ClientState.Ready;
            StartScanning();
            Debugger.LogInfo("Bluetooth is now ready and starts scanning");
        }

        private void CheckToRestartScanning()
        {
            if (_lastScanStarted + SCAN_RESTART_TIME > TimeUtils.Epoch) return;
            
            StopScanning();
            StartScanning();
        }

        private void StartScanning()
        {
            _lastScanStarted = TimeUtils.Epoch;
            BluetoothAccess.StartScanning(PeripheralScanned, new[] { SERVICE_UID });
        }

        private void StopScanning()
        {
            BluetoothAccess.StopScanning();
        }
        
        private void PeripheralScanned(PeripheralInfo peripheral)
        {
            Debugger.LogInfo($"Scanned peripheral {peripheral.Name}");

            if (_connections.Count < MAX_CONNECTIONS)
                ValidateScannedDeviceAndConnect(peripheral);
            else if (RemoveOldestConnectedDevice())
                ValidateScannedDeviceAndConnect(peripheral);
        }

        private enum ClientState
        {
            WaitingForInitialization,
            Initialized,
            Ready
        }

        private void ValidateScannedDeviceAndConnect(PeripheralInfo peripheral)
        {
            // The lamp is already found from network and doesn't need to be added through bluetooth.
            if (LampManager.Instance.GetLampWithSerial<VoyagerLamp>(peripheral.Name) != null)
                return;

            BluetoothAccess.Connect(peripheral.Id,
                SetupValidatedDevice,
                (info, error) => { },
                (info, error) =>
                {
                    var connection = GetConnectionWithId(info.Id);
                    if (connection == null) return;
                    _connections.Remove(connection);
                    _inActiveConnections.Add(connection.Id);
                });
        }

        private void SetupValidatedDevice(PeripheralAccess access)
        {
            var connection = new BluetoothConnection(access);
            
            access.SubscribeToCharacteristic(SERVICE_UID, UART_TX_CHARACTERISTIC_UUID, DataReceivedFromBluetooth);
            
            _connections.Add(connection);
            
            Debugger.LogInfo("Connected to bluetooth");
        }

        private void DataReceivedFromBluetooth(PeripheralAccess access, string service, string characteristic, byte[] data)
        {
            var str = Encoding.UTF8.GetString(data);
            Debugger.LogInfo("Bluetooth received: " + data);
        }

        private bool RemoveOldestConnectedDevice()
        {
            var remove = _connections.FirstOrDefault(c => c.LastMessage > LEFT_OUT_TIME);

            if (remove == null) return false;
            
            BluetoothAccess.Disconnect(remove.Id);
            return true;
        }

        private BluetoothConnection GetConnectionWithId(string id)
        {
            return _connections.FirstOrDefault(c => c.Id == id);
        }

        public override void SendSettingsPacket(VoyagerLamp voyager, Packet packet, double time)
        {
            throw new System.NotImplementedException();
        }

        public override double TimeOffset => 0.0;

        public override void SetItshe(VoyagerLamp voyager, Itshe itshe)
        {
            throw new System.NotImplementedException();
        }

        public override double StartStream(VoyagerLamp voyager)
        {
            throw new System.NotImplementedException();
        }

        public override void SendStreamFrame(VoyagerLamp voyager, double time, double index, Rgb[] frame)
        {
            throw new System.NotImplementedException();
        }

        public override double StartVideo(VoyagerLamp voyager, long frameCount, double startTime = -1)
        {
            throw new System.NotImplementedException();
        }

        public override void SendVideoFrame(VoyagerLamp voyager, long index, double time, Rgb[] frame)
        {
            throw new System.NotImplementedException();
        }

        public override void OverridePixels(VoyagerLamp voyager, Itshe itshe, double duration)
        {
            throw new System.NotImplementedException();
        }

        public override void SetFps(VoyagerLamp voyager, double fps)
        {
            throw new System.NotImplementedException();
        }

        public override void SetNetworkMode(VoyagerLamp voyager, NetworkMode mode, string ssid = "", string password = "")
        {
            throw new System.NotImplementedException();
        }

        public override void SetGlobalIntensity(VoyagerLamp voyager, double value)
        {
            throw new System.NotImplementedException();
        }
        
        private class BluetoothConnection
        {
            public string Id => Access.Id;
            public PeripheralAccess Access { get; }
            public double LastMessage { get; set; }

            public BluetoothConnection(PeripheralAccess access)
            {
                Access = access;
                LastMessage = TimeUtils.Epoch;
            }
        }
    }
}
