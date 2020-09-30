using System.Collections.Generic;
using System.Linq;

namespace DigitalSputnik.Ble
{
    public class BluetoothConnection
    {
        public BluetoothPeripheral Peripheral { get; }
        private readonly PeripheralAccess _access;

        private readonly Dictionary<string, string[]> _characteristics = new Dictionary<string, string[]>();

        public BluetoothConnection(BluetoothPeripheral peripheral, PeripheralAccess access)
        {
            Peripheral = peripheral;
            _access = access;
        }

        public void GetConnectedRssi()
        {
            _access.GetConnectedRssi();
        }

        public void ScanServices(PeripheralServicesScanned scanned)
        {
            _access.ScanServices((_, services) =>
            {
                foreach (var service in services)
                {
                    if (!_characteristics.ContainsKey(service))
                        _characteristics.Add(service, null);

                }
                scanned?.Invoke(this, services);
            });
        }

        public void ScanCharacteristics(string service, PeripheralCharacteristicsScanned scanned)
        {
            _access.ScanServiceCharacteristics(service, (access, _, characteristics) =>
            {
                _characteristics[service] = characteristics;
                scanned?.Invoke(this, service, characteristics);
            });
        }

        public void ListenToCharacteristic(string service, string characteristic, PeripheralMessageReceived received)
        {
            _access.SubscribeToCharacteristic(service, characteristic, (access, s, chara, data) =>
            {
                received?.Invoke(this, characteristic, data);
            });
        }

        public void Write(string characteristic, byte[] data)
        {
            _access.WriteToCharacteristic(characteristic, data);
        }
    }
}