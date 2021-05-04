namespace DigitalSputnik.Ble
{
    public class BluetoothPeripheral
    {
        public string Id { get; }
        public string Name { get; set; } = "Unknown";
        public int Rssi { get; set; }

        public PeripheralUpdated Updated;

        public BluetoothPeripheral(string id)
        {
            Id = id;
        }

        public void UpdateInfo(string name, int rssi)
        {
            Name = name;
            Rssi = rssi;
            Updated?.Invoke(this);
        }

        public void Connect(PeripheralConnected connected, PeripheralConnectionFailed failed, PeripheralDisconnected disconnected)
        {
            BluetoothAccess.Connect(Id, 
                access =>
                {
                    var connection = new BluetoothConnection(this, access);
                    connected?.Invoke(connection);
                },
                (info, error) => failed?.Invoke(this, error),
                (info, error) => disconnected?.Invoke(this, error));
        }

        public void Disconnect()
        {
            BluetoothAccess.Disconnect(Id);
        }
    }
}