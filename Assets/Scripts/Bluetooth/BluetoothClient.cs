using DigitalSputnik;
using DigitalSputnik.Ble;

namespace VoyagerController
{
    public class BluetoothClient : LampClient
    {
        private const double INITIALIZATION_TIME = 0.5;
        private const double SCAN_RESTART_TIME = 30.0;
        private const string SERVICE_UID = "6e400001-b5a3-f393-e0a9-e50e24dcca9e";
        private const string UART_RX_CHARACTERISTIC_UUID = "6e400002-b5a3-f393-e0a9-e50e24dcca9e";
        private const string UART_TX_CHARACTERISTIC_UUID = "6e400003-b5a3-f393-e0a9-e50e24dcca9e";

        private ClientState _state = ClientState.WaitingForInitialization;
        private double _initializedTime = 0.0;
        private double _lastScanStarted = 0.0;
        
        public BluetoothClient()
        {
            BluetoothAccess.Initialize();
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
        }

        private enum ClientState
        {
            WaitingForInitialization,
            Initialized,
            Ready
        }
    }
}
