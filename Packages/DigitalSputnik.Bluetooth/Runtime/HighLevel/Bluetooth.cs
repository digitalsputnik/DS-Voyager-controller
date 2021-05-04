using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DigitalSputnik.Ble
{
    public static class Bluetooth
    {
        private static readonly List<BluetoothPeripheral> _scannedPeripherals = new List<BluetoothPeripheral>();
        private static bool _scanning;
        
        public static void Scan(string[] services, PeripheralScanned scanned)
        {
            _scanning = true;
            BluetoothAccess.StartScanning(peripheral =>
            {
                var peri = PeripheralWithId(peripheral.Id);
                    
                if (peri == null)
                {
                    peri = new BluetoothPeripheral(peripheral.Id)
                    {
                        Name = peripheral.Name,
                        Rssi = peripheral.Rssi
                    };

                    _scannedPeripherals.Add(peri);
                    scanned.Invoke(peri);
                    return;
                }
                
                peri.UpdateInfo(peripheral.Name, peripheral.Rssi);
            }, services);
        }

        public static void EndScanning()
        {
            if (!_scanning) return;
            
            BluetoothAccess.StopScanning();
            _scanning = false;
        }

        private static BluetoothPeripheral PeripheralWithId(string id)
        {
            return _scannedPeripherals.FirstOrDefault(p => p.Id == id);
        }
    }
}