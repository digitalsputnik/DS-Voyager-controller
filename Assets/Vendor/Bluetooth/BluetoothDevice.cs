using System.Collections.Generic;

namespace DigitalSputnik.Bluetooth
{
    public class BluetoothDevice
    {
        public string id;
        public string name;
        public int rssi;

        public List<string> services = new List<string>();
        public Dictionary<string, string> characteristics = new Dictionary<string, string>();

        public bool connected;

        public BluetoothDevice(string _id, string _name, int _rssi)
        {
            id = _id;
            name = _name;
            rssi = _rssi;
        }
    }
}
