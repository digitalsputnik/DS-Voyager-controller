using System.Collections.Generic;

namespace DigitalSputnik.Bluetooth
{
    public class BluetoothDevice
    {
        public string id;
        public string name;
        public int rssi;

        public object device;
        public object gatt;
        public List<Characteristic> characteristics = new List<Characteristic>();
        public List<Service> services = new List<Service>();

        public BluetoothDevice(string _id, string _name, int _rssi, object _device)
        {
            id = _id;
            name = _name;
            rssi = _rssi;
            device = _device;
        }
    }

    public class Characteristic
    {
        public string characteristicUuid;
        public object characteristic;

        public Characteristic(string _characteristicUuid, object _characteristic)
        {
            characteristicUuid = _characteristicUuid;
            characteristic = _characteristic;
        }

        public string GetCharacteristicUuid()
        {
            return characteristicUuid;
        }
        public object GetCharacteristic()
        {
            return characteristic;
        }
    }

    public class Service
    {
        public string serviceUuid;
        public object service;

        public Service(string _serviceUuid, object _service)
        {
            serviceUuid = _serviceUuid;
            service = _service;
        }

        public string GetServiceUuid()
        {
            return serviceUuid;
        }
        public object GetService()
        {
            return service;
        }
    }
}
