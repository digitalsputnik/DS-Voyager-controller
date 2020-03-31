namespace DigitalSputnik.Bluetooth
{
    internal interface IBluetoothInterfaceTest
    {
        void Initialize();
        void EnableBluetooth();
        void DisableBluetooth();
        void StartScanning(string[] services, InternalPeripheralScanHandlerTest callback);
        void StopScanning();
        void Connect(object device, InternalPeripheralConnectHandlerTest onConnect, InternalPeripheralConnectFailHandlerTest onFail, InternalPeripheralDisconnectHandlerTest onDisconnect);
        void Disconnect(object gatt);
        void GetServices(object gatt, InternalServicesHandlerTest callback);
        void GetCharacteristic(string id, object service, string uuid, InternalCharacteristicHandlerTest callback);
        void SetCharacteristicsUpdateCallback(InternalCharacteristicUpdateHandlerTest callback);
        void SubscribeToCharacteristicUpdate(object gatt, object characteristic);
        void WriteToCharacteristic(object gatt, object characteristic, byte[] data);
    }

    internal delegate void InternalPeripheralScanHandlerTest(string id, string name, int rssi, object device);

    internal delegate void InternalPeripheralConnectHandlerTest(string id, object gatt);

    internal delegate void InternalPeripheralConnectFailHandlerTest(string id, string error);

    internal delegate void InternalPeripheralDisconnectHandlerTest(string id, string error);

    internal delegate void InternalServicesHandlerTest(string id, string serviceUuid, object service);

    internal delegate void InternalCharacteristicHandlerTest(string id, string characteristicUuid, object characteristic);

    internal delegate void InternalCharacteristicUpdateHandlerTest(string id, object characteristic, int status, string message);
}