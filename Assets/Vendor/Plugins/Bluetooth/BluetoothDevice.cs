using System.Collections.Generic;

namespace DigitalSputnik.Bluetooth
{
    public class BluetoothDevice
    {
        public string id;
        public string name;
        public int rssi;

        public BluetoothConnection connection;

        public string[] services;
        public Dictionary<string, string> characteristics = new Dictionary<string, string>();

        public bool connected;

        public BluetoothDevice(string _id, string _name, int _rssi, BluetoothConnection _connection)
        {
            id = _id;
            name = _name;
            rssi = _rssi;
            connection = _connection;
        }
    }
}
