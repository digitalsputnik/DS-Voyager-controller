namespace DigitalSputnik.Bluetooth
{
    internal interface IBluetoothInterfaceTest
    {
        void Initialize();
        void EnableBluetooth();
        void DisableBluetooth();
        void StartScanning(string[] services, InternalPeripheralScanHandlerTest callback);
        void StopScanning();
        void Connect(string id, InternalPeripheralConnectHandlerTest onConnect, InternalPeripheralConnectFailHandlerTest onFail, InternalPeripheralDisconnectHandlerTest onDisconnect);
        void Disconnect(string id);
        void GetServices(string id, InternalServicesHandlerTest callback);
        void GetCharacteristic(string id, string service, string uuid, InternalCharacteristicHandlerTest callback);
        void SetCharacteristicsUpdateCallback(InternalCharacteristicUpdateHandlerTest callback);
        void SubscribeToCharacteristicUpdate(string id, string characteristic);
        void WriteToCharacteristic(string id, string characteristic, byte[] data);
    }

    internal delegate void InternalPeripheralScanHandlerTest(string id, string name, int rssi);

    internal delegate void InternalPeripheralConnectHandlerTest(string id);

    internal delegate void InternalPeripheralConnectFailHandlerTest(string id, string error);

    internal delegate void InternalPeripheralDisconnectHandlerTest(string id, string error);

    internal delegate void InternalServicesHandlerTest(string id, string service);

    internal delegate void InternalCharacteristicHandlerTest(string id, string characteristic);

    internal delegate void InternalCharacteristicUpdateHandlerTest(string id, int status, string message);
}