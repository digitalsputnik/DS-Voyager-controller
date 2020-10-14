using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DigitalSputnik;
using DigitalSputnik.Ble;
using DigitalSputnik.Colors;
using DigitalSputnik.Voyager;
using DigitalSputnik.Voyager.Communication;
using DigitalSputnik.Voyager.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace VoyagerController
{
    public class VoyagerBluetoothClient : VoyagerClient
    {
        private static VoyagerBluetoothClient _instance;
        
        private const double INITIALIZATION_TIME = 2.0;
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
        // private readonly List<string> _inActiveConnections = new List<string>();
        
        public VoyagerBluetoothClient()
        {
            BluetoothAccess.Initialize();
            _instance = this; 
        }

        private static void SendMessage(Lamp lamp, byte[] message)
        {
            MainThread.Dispatch(() =>
            {
                var connection = _instance._connections.FirstOrDefault(c => c.Lamp == lamp);
                connection?.Access.WriteToCharacteristic(UART_RX_CHARACTERISTIC_UUID, message);

                if (connection != null)
                    Debugger.LogInfo($"Send message to {lamp.Serial}: {Encoding.UTF8.GetString(message)}");
            });
        }

        protected override void Update()
        {
            switch (_state)
            {
                case ClientState.WaitingForInitialization when BluetoothAccess.IsInitialized:
                    OnBluetoothInitializedByDevice();
                    break;
                case ClientState.Initialized when TimeUtils.Epoch > _initializedTime + INITIALIZATION_TIME && BluetoothAccess.IsInitialized:
                    OnBluetoothWaitedByController();
                    break;
                case ClientState.Ready:
                    CheckToRestartScanning();
                    break;
            }

            foreach (var connection in _connections)
            {
                if (!(TimeUtils.Epoch > connection.LastMessage + 0.5)) continue;

                switch (connection.State)
                {
                    case ValidateState.GettingSerial:
                        RequestInfo(connection.Lamp, "get_serial");
                        break;
                    case ValidateState.GettingLength:
                        RequestInfo(connection.Lamp, "get_length");
                        break;
                }
                
                connection.LastMessage = TimeUtils.Epoch;
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
            Debugger.LogInfo("Bluetooth is now ready and starts scanning");
            MainThread.Dispatch(StartScanning);
        }

        private void CheckToRestartScanning()
        {
            if (TimeUtils.Epoch < _lastScanStarted + SCAN_RESTART_TIME) return;
            
            MainThread.Dispatch(() =>
            {
                StopScanning();
                StartScanning();
            });

            _lastScanStarted = TimeUtils.Epoch;
        }

        private void StartScanning()
        {
            Debugger.LogInfo("Bluetooth scanning started");
            _lastScanStarted = TimeUtils.Epoch;
            BluetoothAccess.StartScanning(PeripheralScanned, new[] { SERVICE_UID });
        }

        private void StopScanning()
        {
            BluetoothAccess.StopScanning();
        }
        
        private void PeripheralScanned(PeripheralInfo peripheral)
        {
            Debugger.LogWarning($"Scanned peripheral {peripheral.Name}");

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
            
            Debugger.LogInfo($"Peripheral found - {peripheral.Name}");

            BluetoothAccess.Connect(peripheral.Id,
                SetupValidatedDevice,
                (info, error) => { },
                (info, error) =>
                {
                    var connection = GetConnectionWithId(info.Id);
                    if (connection == null) return;
                    _connections.Remove(connection);
                    // _inActiveConnections.Add(connection.Id);
                });
        }

        private void SetupValidatedDevice(PeripheralAccess access)
        {
            access.ScanServices(ServicesScanned);
        }

        private void ServicesScanned(PeripheralAccess access, string[] services)
        {
            access.ScanServiceCharacteristics(SERVICE_UID, CharacteristicsScanned);
        }

        private void CharacteristicsScanned(PeripheralAccess access, string service, string[] characteristics)
        {
            var lamp = new VoyagerLamp(this) { Endpoint = new BluetoothEndPoint(access.Id) };
            var connection = new BluetoothConnection(access, lamp) { State = ValidateState.GettingSerial };
            access.SubscribeToCharacteristic(SERVICE_UID, UART_TX_CHARACTERISTIC_UUID, DataReceivedFromBluetooth);
            _connections.Add(connection);
        }

        private static void RequestInfo(Lamp lamp, string request)
        {
            var package = new RequestPackage(request);
            SendMessage(lamp, ObjectToBytes(package));
        }

        private void DataReceivedFromBluetooth(PeripheralAccess access, string service, string characteristic, byte[] data)
        {
            var str = Encoding.UTF8.GetString(data);
            var connection = _connections.FirstOrDefault(c => c.Access == access);

            if (connection == null) return;
            
            Debugger.LogInfo("Bluetooth received: " + str);

            if (!str.StartsWith("{") || !str.EndsWith("}")) return;
            
            var json = JObject.Parse(str);
                
            if (!json.ContainsKey("op_code")) return;
                
            var op = json.GetValue("op_code")?.ToString();

            switch (op)
            {
                case "get_serial":
                    if (connection.State != ValidateState.GettingSerial) break;
                    var serial = json.GetValue("serial")?.ToString();
                    connection.Lamp.Serial = serial;
                    connection.State = ValidateState.GettingLength;
                    connection.LastMessage = 0.0;
                    break;
                case "get_length":
                    if (connection.State != ValidateState.GettingLength) break;
                    var length = int.Parse(json.GetValue("length")?.ToString() ?? "0");
                    connection.Lamp.PixelCount = length;
                    connection.State = ValidateState.Ready;
                    connection.LastMessage = 0.0;
                    AddLampToManager(connection.Lamp);
                    break;
            }
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
            packet.Timestamp = time;
            SendMessage(voyager, ObjectToBytes(packet));
        }

        public override double TimeOffset => 0.0;

        public override void SetItshe(VoyagerLamp voyager, Itshe itshe)
        {
            var packet = new SetItshePacket("set_itshe", TimeUtils.Epoch, itshe);
            SendMessage(voyager, ObjectToBytes(packet));
        }

        public override double StartStream(VoyagerLamp voyager)
        {
            var time = TimeUtils.Epoch;
            var packet = new SetStreamPacket();
            SendSettingsPacket(voyager, packet, time);
            return time;
        }

        public override void SendStreamFrame(VoyagerLamp voyager, double time, double index, Rgb[] frame)
        {
            var frameData = ColorUtils.RgbArrayToBytes(frame);
            var packet = new StreamFramePacket(index, frameData);
            SendSettingsPacket(voyager, packet, time);
        }

        public override double StartVideo(VoyagerLamp voyager, long frameCount, double startTime = -1)
        {
            var time = TimeUtils.Epoch;
            var packet = new SetVideoPacket(frameCount, startTime);
            SendSettingsPacket(voyager, packet, time);
            return time;
        }

        public override void SendVideoFrame(VoyagerLamp voyager, long index, double time, Rgb[] frame)
        {
            var frameData = ColorUtils.RgbArrayToBytes(frame);
            var packet = new VideoFramePacket(index, frameData);
            SendSettingsPacket(voyager, packet, time);
        }

        public override void OverridePixels(VoyagerLamp voyager, Itshe itshe, double duration)
        {
            throw new NotImplementedException();
        }

        public override void SetFps(VoyagerLamp voyager, double fps)
        {
            var packet = new SetFpsPacket(fps);
            SendSettingsPacket(voyager, packet, TimeUtils.Epoch);
        }

        public override void SetNetworkMode(VoyagerLamp voyager, NetworkMode mode, string ssid = "", string password = "")
        {
            throw new System.NotImplementedException();
        }

        public override void SetGlobalIntensity(VoyagerLamp voyager, double value)
        {
            throw new System.NotImplementedException();
        }

        private static byte[] ObjectToBytes(object obj)
        {
            var json = JsonConvert.SerializeObject(obj, new ItsheConverter());
            return Encoding.UTF8.GetBytes(json);
        }
        
        private class BluetoothConnection
        {
            public string Id => Access.Id;
            public PeripheralAccess Access { get; }
            public double LastMessage { get; set; }
            public VoyagerLamp Lamp { get; }
            public ValidateState State { get; set; }
            
            public BluetoothConnection(PeripheralAccess access, VoyagerLamp lamp)
            {
                Access = access;
                LastMessage = TimeUtils.Epoch;
                Lamp = lamp;
            }
        }

        [Serializable]
        private struct RequestPackage
        {
            [JsonProperty("op_code")]
            public string OpCode { get; set; }

            public RequestPackage(string op) => OpCode = op;
        }

        [Serializable]
        private struct GetSerialResponsePackage
        {
            [JsonProperty("op_code")]
            public string OpCode { get; set; }
            [JsonProperty("serial")]
            public string Serial { get; set; }
        }

        [Serializable]
        private struct GetLengthResponsePackage
        {
            [JsonProperty("op_code")]
            public string OpCode { get; set; }
            [JsonProperty("length")]
            public int Length { get; set; }
        }

        public struct SetItshePacket
        {
            [JsonProperty("op_code")]
            public string OpCode { get; set; }
            [JsonProperty("timestamp")]
            public double Timestamp { get; set; }
            [JsonProperty("itshe")]
            public Itshe Itshe { get; set; }

            public SetItshePacket(string op, double timestamp, Itshe itshe)
            {
                OpCode = op;
                Timestamp = timestamp;
                Itshe = itshe;
            }
        }

        private enum ValidateState
        {
            Connected,
            GettingSerial,
            GettingLength,
            Ready
        }
    }
}
