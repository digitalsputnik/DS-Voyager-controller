using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DigitalSputnik;
using DigitalSputnik.Ble;
using DigitalSputnik.Colors;
using DigitalSputnik.Dmx;
using DigitalSputnik.Voyager;
using DigitalSputnik.Voyager.Communication;
using DigitalSputnik.Voyager.Json;
using Newtonsoft.Json;
using UnityEngine;
using VoyagerController.Workspace;
using PlayMode = DigitalSputnik.Voyager.PlayMode;

namespace VoyagerController.Bluetooth
{
    public class VoyagerBluetoothClient : VoyagerClient
    {
        private static VoyagerBluetoothClient _instance;
        
        private const double INITIALIZATION_TIME = 2.0;
        private const double SCAN_RESTART_TIME = 30.0;
        private const string SERVICE_UID = "6e400001-b5a3-f393-e0a9-e50e24dcca9e";
        private const string UART_RX_CHARACTERISTIC_UUID = "6e400002-b5a3-f393-e0a9-e50e24dcca9e";
        private const string UART_TX_CHARACTERISTIC_UUID = "6e400003-b5a3-f393-e0a9-e50e24dcca9e";
        private const int MAX_CONNECTING = 2;

        private ClientState _state = ClientState.WaitingForInitialization;
        private double _initializedTime = 0.0;
        private double _lastScanStarted = 0.0;
        
        private BleMessageHandler OnBleMessageWithoutOp;
        public override Type EndpointType => typeof(BluetoothEndPoint);

        private readonly PacketParser _packetParser = new PacketParser();
        private readonly CallbackSystem<VoyagerLamp, OpCode> _callbacks = new CallbackSystem<VoyagerLamp, OpCode>();
        private readonly Queue<PeripheralInfo> _connectionsQueue = new Queue<PeripheralInfo>();
        private readonly List<PeripheralInfo> _connectingDevices = new List<PeripheralInfo>();
        private readonly List<BluetoothConnection> _connections = new List<BluetoothConnection>();
        private readonly Dictionary<VoyagerLamp, SsidListHandler> _ssidCallbacks = new Dictionary<VoyagerLamp, SsidListHandler>();
        private readonly Dictionary<BluetoothConnection, List<string>> _ssidLists = new Dictionary<BluetoothConnection, List<string>>();

        private bool CanConnectMoreLamps => _connectingDevices.Count <= MAX_CONNECTING;

        public VoyagerBluetoothClient()
        {
            BluetoothAccess.Initialize();
            _instance = this;

            SubscribeToWorkspaceEvents();

            OnBleMessageWithoutOp += SsidListResponseReceived;
            LampManager.Instance.OnLampUpdated += VoyagerLampUpdated;
            AddOpListener<PollReplyShortPacket>(OpCode.PollReplyS, PollReceived);
        }

        private void SubscribeToWorkspaceEvents()
        {
            WorkspaceManager.ItemAdded += ItemAdded;
            WorkspaceManager.ItemRemoved += ItemRemoved;
        }

        private void ItemAdded(WorkspaceItem item)
        {
            if (item is VoyagerItem voyagerItem)
            {
                if (voyagerItem.LampHandle.Endpoint is BluetoothEndPoint endPoint)
                {
                    switch (Application.platform)
                    {
                        case RuntimePlatform.Android:
                            BluetoothAccess.Reconnect(endPoint.Id);
                            break;
                        case RuntimePlatform.IPhonePlayer:
                        {
                            BluetoothAccess.Connect(endPoint.Id,
                                SetupValidatedDevice,
                                HandleFailedConnection,
                                HandleDisconnection);
                            break;
                        }
                    }
                }
            }
        }

        private void ItemRemoved(WorkspaceItem item)
        {
            if (item is VoyagerItem voyagerItem)
            {
                if (voyagerItem.LampHandle.Endpoint is BluetoothEndPoint endPoint)
                    BluetoothAccess.Disconnect(endPoint.Id);
            }
        }

        private void VoyagerLampUpdated(Lamp lamp)
        {
            /*if (lamp.Endpoint is LampNetworkEndPoint && lamp.Connected)
            {
                var connection = _connections.FirstOrDefault(c => c.Lamp == lamp);

                if (connection != null)
                    BluetoothAccess.Disconnect(connection.Id);
            }*/
        }

        private static void SendMessage(Lamp lamp, byte[] message)
        {
            MainThread.Dispatch(() =>
            {
                var connection = _instance._connections.FirstOrDefault(c => c.Lamp == lamp);
                connection?.Access.WriteToCharacteristic(UART_RX_CHARACTERISTIC_UUID, message);

                if (connection != null)
                    DebugConsole.LogInfo($"Send message to {lamp.Serial}: {Encoding.UTF8.GetString(message)}");
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
                    SetupLamps();
                    break;
            }

            foreach (var connection in _connections.ToList())
            {
                if (!(TimeUtils.Epoch > connection.LastMessage + 0.5)) continue;

                if (connection.ValidationState == ValidateState.GettingInfo)
                    RequestInfo(connection.Lamp);
                
                connection.LastMessage = TimeUtils.Epoch;
            }

            base.Update();
        }

        private void OnBluetoothInitializedByDevice()
        {
            _state = ClientState.Initialized;
            _initializedTime = TimeUtils.Epoch;
            DebugConsole.LogInfo("Bluetooth is initialized by device");
        }

        private void OnBluetoothWaitedByController()
        {
            _state = ClientState.Ready;
            DebugConsole.LogInfo("Bluetooth is now ready and starts scanning");
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
            DebugConsole.LogInfo("Bluetooth scanning started");
            _lastScanStarted = TimeUtils.Epoch;
            BluetoothAccess.StartScanning(PeripheralScanned, new[] { SERVICE_UID });
        }

        private void StopScanning()
        {
            BluetoothAccess.StopScanning();
        }
        
        private void PeripheralScanned(PeripheralInfo peripheral)
        {
            DebugConsole.LogWarning($"Scanned peripheral {peripheral.Name}");
            _connectionsQueue.Enqueue(peripheral);
        }

        private void SetupLamps()
        {
            if (!CanConnectMoreLamps)
                return;

            if (_connectionsQueue.Count == 0)
                return;

            var peripheral = _connectionsQueue.Dequeue();

            MainThread.Dispatch(() => ValidateScannedDeviceAndConnect(peripheral));
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

            var updLamp = LampManager.Instance.GetLampWithSerial<VoyagerLamp>(peripheral.Name);
            if (updLamp != null && updLamp.Connected) return;

            DebugConsole.LogInfo($"Connecting - {peripheral.Name}");
            
            _connectingDevices.Add(peripheral);

            BluetoothAccess.Connect(peripheral.Id,
                SetupValidatedDevice,
                HandleFailedConnection,
                HandleDisconnection);
        }

        private void SetupValidatedDevice(PeripheralAccess access)
        {
            DebugConsole.LogInfo("Connected - " + access.Id);

            var connection = GetConnectionWithId(access.Id);

            if (connection == null)
                access.ScanServices(ServicesScanned);
            else
            {
                if (Application.platform == RuntimePlatform.Android)
                {
                    connection.Lamp.Connected = true;
                    connection.ConnectionState = ConnectionState.Connected;
                    connection.Access = access;
                    connection.Access.SubscribeToCharacteristic(SERVICE_UID, UART_TX_CHARACTERISTIC_UUID, DataReceivedFromBluetooth);   
                }
                else
                {
                    access.ScanServices(ScannedServicesToApprovedConnection);
                }
            }
        }

        private void ScannedServicesToApprovedConnection(PeripheralAccess access, string[] services)
        {
            access.ScanServiceCharacteristics(SERVICE_UID, ScannedCharacteristicsToApprovedConnection);
        }

        private void ScannedCharacteristicsToApprovedConnection(PeripheralAccess access, string service, string[] characteristics)
        {
            var connection = GetConnectionWithId(access.Id);
            connection.Lamp.Connected = true;
            connection.ConnectionState = ConnectionState.Connected;
            connection.Access = access;
            access.SubscribeToCharacteristic(SERVICE_UID, UART_TX_CHARACTERISTIC_UUID, DataReceivedFromBluetooth);
        }

        private void HandleFailedConnection(PeripheralInfo info, string error)
        {
            DebugConsole.LogInfo("Failed Connection - " + info.Name);

            var peripheral = _connectingDevices.FirstOrDefault(l => l.Id == info.Id);

            if (peripheral != null)
                _connectingDevices.Remove(peripheral);

            var connection = GetConnectionWithId(info.Id);

            if (connection == null)
            {
                _connectionsQueue.Enqueue(info);
                return;
            }

            connection.Lamp.Connected = false;
            connection.ConnectionState = ConnectionState.Disconnected;
        }

        private void HandleDisconnection(PeripheralInfo info, string error)
        {
            DebugConsole.LogInfo("Disconnected - " + info.Name);

            var peripheral = _connectingDevices.FirstOrDefault(l => l.Id == info.Id);

            if (peripheral != null)
                _connectingDevices.Remove(peripheral);

            var connection = GetConnectionWithId(info.Id);

            if (connection == null)
            {
                _connectionsQueue.Enqueue(info);
                return;
            }

            if (connection.Lamp.Endpoint is BluetoothEndPoint)
                connection.Lamp.Connected = false;

            connection.ConnectionState = ConnectionState.Disconnected;
        }

        private void ServicesScanned(PeripheralAccess access, string[] services)
        {
            access.ScanServiceCharacteristics(SERVICE_UID, CharacteristicsScanned);
        }

        private void CharacteristicsScanned(PeripheralAccess access, string service, string[] characteristics)
        {
            var lamp = new VoyagerLamp { Endpoint = new BluetoothEndPoint(access.Id) };
            var connection = new BluetoothConnection(access, lamp) { ValidationState = ValidateState.GettingInfo, ConnectionState = ConnectionState.Connected };
            access.SubscribeToCharacteristic(SERVICE_UID, UART_TX_CHARACTERISTIC_UUID, DataReceivedFromBluetooth);
            _connections.Add(connection);
        }

        private static void RequestInfo(Lamp lamp)
        {
            var package = new PollRequestShortPacket();
            SendMessage(lamp, ObjectToBytes(package));
        }

        private void PollReceived(VoyagerLamp sender, PollReplyShortPacket data)
        {
            var connection = _connections.FirstOrDefault(c => c.Lamp == sender);

            connection.Lamp.Serial = data.Serial;
            connection.Lamp.PixelCount = data.Length;
            connection.Lamp.Version = data.ChipVersion[0] + "." + data.ChipVersion[1];
            connection.ValidationState = ValidateState.Ready;
            connection.Lamp.Connected = false;
            connection.LastMessage = 0.0;

            var peripheral = _connectingDevices.FirstOrDefault(l => l.Id == connection.Id);

            if (peripheral != null)
                _connectingDevices.Remove(peripheral);

            BluetoothAccess.Disconnect(connection.Id);
            AddLampToManager(connection.Lamp);
        }

        private void DataReceivedFromBluetooth(PeripheralAccess access, string service, string characteristic, byte[] data)
        {
            var json = Encoding.UTF8.GetString(data);
            
            Debug.Log(json);
            
            var connection = _connections.FirstOrDefault(c => c.Access == access);

            if (connection == null) return;

            var op = Packet.GetOpCode(json);

            OnBleMessageWithoutOp?.Invoke(connection, json);

            if (op == OpCode.None || !_callbacks.Contains(op)) return;

            var packet = _packetParser.Deserialize(_callbacks.GetKeyType(op), json);
            var lamp = connection.Lamp;

            if (connection != null) connection.LastMessage = TimeUtils.Epoch;

            _callbacks.Invoke(op, lamp, packet);
        }

        private BluetoothConnection GetConnectionWithId(string id)
        {
            return _connections.FirstOrDefault(c => c.Id == id);
        }

        #region Implementaion

        public override void PollAvailableSsidList(VoyagerLamp voyager, SsidListHandler callback)
        {
            //TODO check if lamp is supported, based on firmware version 

            AddLampToSsidReplyList(voyager, callback);

            var packet = new SsidListRequestPacket();
            var time = TimeUtils.Epoch + TimeOffset;

            SendSettingsPacket(voyager, packet, time);
        }

        private void AddLampToSsidReplyList(VoyagerLamp voyager, SsidListHandler callback)
        {
            if (!_ssidCallbacks.ContainsKey(voyager))
                _ssidCallbacks.Add(voyager, callback);
            else
            {
                _ssidCallbacks[voyager]?.Invoke(voyager, new string[0]);
                _ssidCallbacks[voyager] = callback;
            }
        }

        private void SsidListResponseReceived(BluetoothConnection connection, string json)
        {
            const string BEGIN_JSON = "{\"op_code\": \"ack_ssid_list_request\"}";
            const string END_JSON = "{\"op_code\": \"ack_ssid_list_complete\"}";

            if (json == BEGIN_JSON && !_ssidLists.ContainsKey(connection))
                _ssidLists.Add(connection, new List<string>());

            if (json != BEGIN_JSON && json != END_JSON && _ssidLists.ContainsKey(connection))
            {
                foreach (var ssid in json.Split(','))
                {
                    if (!_ssidLists[connection].Contains(ssid))
                        _ssidLists[connection].Add(ssid);
                }
            }

            if (json == END_JSON)
            {
                if (_ssidCallbacks.ContainsKey(connection.Lamp) && _ssidLists.ContainsKey(connection))
                {
                    _ssidCallbacks[connection.Lamp]?.Invoke(connection.Lamp, _ssidLists[connection].ToArray());
                    _ssidCallbacks.Remove(connection.Lamp);
                    _ssidLists.Remove(connection);
                }
            }
        }

        public void AddOpListener<T>(OpCode op, CallbackHandler<VoyagerLamp, T> callback) where T : Packet
        {
            _callbacks.AddListener(op, callback);
        }

        public void RemoveOpListener<T>(OpCode op, CallbackHandler<VoyagerLamp, T> callback) where T : Packet
        {
            _callbacks.RemoveListener(op, callback);
        }

        public override double TimeOffset => 0.0;

        public override void SendSettingsPacket(VoyagerLamp voyager, Packet packet, double time)
        {
            packet.Timestamp = time;
            SendMessage(voyager, ObjectToBytes(packet));
        }

        public override void SendDiscoveryPacket(VoyagerLamp voyager, Packet packet, double time)
        {
            packet.Timestamp = time;
            SendMessage(voyager, ObjectToBytes(packet));
        }

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
            var packet = new PixelOverridePacket(itshe, duration);
            var time = TimeUtils.Epoch + TimeOffset;
            SendSettingsPacket(voyager, packet, time);
        }

        public override void SetFps(VoyagerLamp voyager, double fps)
        {
            var packet = new SetFpsPacket(fps);
            SendSettingsPacket(voyager, packet, TimeUtils.Epoch);
        }

        public override void SetNetworkMode(VoyagerLamp voyager, NetworkMode mode, string ssid = "", string password = "")
        {
            //TODO check if psk encryption is supported on lamp, based on firmware version 

            if (mode == NetworkMode.ClientPSK)
                password = SecurityUtils.WPA_PSK(ssid, password);
            
            var packet = new NetworkModeRequestPacket(voyager.Serial, mode, ssid, password);
            var time = TimeUtils.Epoch + TimeOffset;
            
            for (var i = 0; i < 5; i++)
                SendDiscoveryPacket(voyager, packet, time);
        }

        public override void SetPlaymode(VoyagerLamp voyager, PlayMode mode, double startTime, double handle)
        {
            
        }

        public override void SetGlobalIntensity(VoyagerLamp voyager, double value)
        {
            
        }
        
        public override void SetDmxMode(VoyagerLamp voyager, DmxSettings settings)
        {
            
        }

        private static byte[] ObjectToBytes(object obj)
        {
            var converters = new JsonConverter[]
            {
                new ItsheConverter(),
                new NetworkModeConverter()
            };

            var json = JsonConvert.SerializeObject(obj, converters);
            return Encoding.UTF8.GetBytes(json);
        }

        #endregion

        private enum ValidateState
        {
            Connected,
            GettingInfo,
            Ready
        }

        private enum ConnectionState
        {
            Connected,
            Disconnected,
            Closed
        }

        private class BluetoothConnection
        {
            public string Id => Access.Id;
            public PeripheralAccess Access { get; set; }
            public double LastMessage { get; set; }
            public VoyagerLamp Lamp { get; }
            public ValidateState ValidationState { get; set; }
            public ConnectionState ConnectionState { get; set; }

            public BluetoothConnection(PeripheralAccess access, VoyagerLamp lamp)
            {
                Access = access;
                LastMessage = TimeUtils.Epoch;
                Lamp = lamp;
            }
        }

        private class BleNetworkModeRequest
        {
            [JsonProperty("op_code", Order = -3)]
            public OpCode OpCode = OpCode.NetworkModeRequest;
            [JsonProperty("network_mode")]
            public NetworkMode Mode;
            [JsonProperty("set_pattern")]
            public string Ssid;
            [JsonProperty("set_pattern_ps")]
            public string Password;

            public BleNetworkModeRequest(NetworkMode mode, string ssid, string password)
            {
                Mode = mode;
                Ssid = ssid;
                Password = password;
            }
        }

        private class BleNetworkModeRequestPsk
        {
            [JsonProperty("op_code", Order = -3)]
            public string OpCode = "bl_nmr";
            [JsonProperty("mode")]
            public int Mode = 3;
            [JsonProperty("u")]
            public string Ssid;
            [JsonProperty("p")]
            public string Password;

            public BleNetworkModeRequestPsk(string ssid, string password)
            {
                Ssid = ssid;
                Password = password;
            }
        }

        private delegate void BleMessageHandler(BluetoothConnection connection, string data);
    }
}
