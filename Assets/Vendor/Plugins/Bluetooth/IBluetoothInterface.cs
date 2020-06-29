// -----------------------------------------------------------------
// Author: Taavet Maask	Date: 11/20/2019
// Copyright: © Digital Sputnik OÜ
// -----------------------------------------------------------------

namespace DigitalSputnik.Bluetooth
{
    internal interface IBluetoothInterface
    {
        bool IsInitialized();
        void Initialize();
        void EnableBluetooth();
        void DisableBluetooth();
        void StartScanning(string[] services, InternalPeripheralScanHandler callback);
        void StopScanning();
        void Connect(string id, InternalPeripheralConnectHandler onConnect, InternalPeripheralConnectFailHandler onFail, InternalPeripheralDisconnectHandler onDisconnect);
        void Disconnect(string id);
        void GetServices(string id, InternalServicesHandler callback);
        void GetCharacteristics(string id, string service, InternalCharacteristicHandler callback);
        void SetCharacteristicsUpdateCallback(string id, InternalCharacteristicUpdateHandler callback);
        void SubscribeToCharacteristicUpdate(string id, string characteristic);
        void WriteToCharacteristic(string id, string characteristic, byte[] data);
    }

    internal delegate void InternalPeripheralScanHandler(string id, string name, int rssi);

    internal delegate void InternalPeripheralConnectHandler(string id);

    internal delegate void InternalPeripheralConnectFailHandler(string id, string error);

    internal delegate void InternalPeripheralDisconnectHandler(string id, string error);

    internal delegate void InternalServicesHandler(string id, string[] services);

    internal delegate void InternalCharacteristicHandler(string id, string service, string[] characters);

    internal delegate void InternalCharacteristicUpdateHandler(string id, string service, string characteristic, byte[] data);
}