namespace DigitalSputnik.Ble
{
    public delegate void PeripheralScanned(BluetoothPeripheral peripheral);
    public delegate void PeripheralUpdated(BluetoothPeripheral peripheral);
    public delegate void PeripheralConnected(BluetoothConnection connection);
    public delegate void PeripheralConnectionFailed(BluetoothPeripheral peripheral, string error);
    public delegate void PeripheralDisconnected(BluetoothPeripheral peripheral, string error);
    public delegate void PeripheralServicesScanned(BluetoothConnection connection, string[] services);
    public delegate void PeripheralCharacteristicsScanned(BluetoothConnection connection, string service, string[] characteristics);
    public delegate void PeripheralMessageReceived(BluetoothConnection connection, string characteristic, byte[] data);
}