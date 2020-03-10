using DigitalSputnik.Bluetooth;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace VoyagerApp.UI.Menus
{
    public class BLEItem : MonoBehaviour
    {
        [SerializeField] Text serialText = null;
        [SerializeField] Text signalText = null;

        public DateTime lastScan;

        public string id;
        public string serial;
        public string rssi;

        public bool selected = false;
        public bool isConnecting = false;
        public bool connected = false;

        public PeripheralInfo peripheral;
        public PeripheralAccess peripheralAccess;
        public BluetoothConnection connection;
        public AndroidJavaObject androidDevice;

        public void SetPeripheral(PeripheralInfo peripheral)
        {
            this.peripheral = peripheral;
            id = peripheral.id;
            rssi = peripheral.rssi.ToString();
            serial = peripheral.name;
            serialText.text = serial;
            signalText.text = rssi;
            lastScan = DateTime.Now;
        }

        public void OnClick()
        {
            if(BluetoothTest.instance.bleItems.Where(l => l.selected).Count() < 5)
            {
                selected = !selected;

                var btn = GetComponent<Button>();
                ColorBlock btnColor = btn.colors;

                if (selected)
                {
                    btnColor.selectedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
                    btnColor.normalColor = new Color(0.8f, 0.8f, 0.8f, 1f);
                }
                else
                {
                    btnColor.selectedColor = Color.white;
                    btnColor.normalColor = Color.white;
                }

                btn.colors = btnColor;
            }
        }

        public void UpdateInfo(PeripheralInfo peripheral)
        {
            rssi = peripheral.rssi.ToString();
            signalText.text = rssi;
            lastScan = DateTime.Now;
        }
    }
}