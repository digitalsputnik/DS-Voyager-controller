using DigitalSputnik.Bluetooth;
using UnityEngine;
using UnityEngine.UI;

namespace VoyagerApp.UI.Menus
{
    public class BLEItem : MonoBehaviour
    {
        [SerializeField] Text serialText = null;
        [SerializeField] Text signalText = null;

        public string id;
        public string serial;
        public string rssi;

        public bool selected = false;

        public bool connected = false;

        public PeripheralInfo peripheral;
        public PeripheralAccess peripheralAccess;
        public BluetoothConnection connection;
        public AndroidJavaObject device;

        public void SetPeripheral(PeripheralInfo peripheral)
        {
            this.peripheral = peripheral;
            id = peripheral.id;
            rssi = peripheral.rssi.ToString();
            serial = peripheral.name;
            serialText.text = serial;
            signalText.text = rssi;
        }

        public void OnClick()
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
}