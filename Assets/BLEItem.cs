using DigitalSputnik.Bluetooth;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using VoyagerApp.Networking.Voyager;

namespace VoyagerApp.UI.Menus
{
    public class BLEItem : MonoBehaviour
    {
        string SERVICE_UID = "6e400001-b5a3-f393-e0a9-e50e24dcca9e";
        string UART_RX_CHARACTERISTIC_UUID = "6e400002-b5a3-f393-e0a9-e50e24dcca9e";
        string UART_TX_CHARACTERISTIC_UUID = "6e400003-b5a3-f393-e0a9-e50e24dcca9e";

        [SerializeField] Text serialText = null;
        [SerializeField] Text rssiText = null;

        public PeripheralInfo peripheral;
        public BluetoothDevice device;

        public bool selected = false;
        public bool connecting = false;
        public bool connected = false;

        BluetoothTest instance;

        public void SetPeripheral(PeripheralInfo _peripheral, BluetoothTest _instance)
        {
            peripheral = _peripheral;
            instance = _instance;

            serialText.text = peripheral.name;
            rssiText.text = peripheral.rssi.ToString();
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

            if (selected)
            {
                instance.ResetAllItems();
                Connect();
            }
            else
            {
                Disconnect();
                device.connection.HandleDisconnection();
            }
        }

        public void ResetItem()
        {
            Disconnect();
            device.connection.HandleDisconnection();

            selected = false;
            connecting = false;
            connected = false;

            var btn = GetComponent<Button>();
            ColorBlock btnColor = btn.colors;

            btnColor.selectedColor = Color.white;
            btnColor.normalColor = Color.white;

            btn.colors = btnColor;
        }

        void Connect()
        {
            device = null;
            Debug.Log($"Connecting to lamp {peripheral.id}");
            instance.statusText.text = $"Connecting to lamp {peripheral.id}";
            connecting = true;
            BluetoothHelper.ConnectToPeripheral(peripheral.id, OnConnected, OnFailed, OnDisconnected);
        }

        void Disconnect()
        {
            BluetoothHelper.DisconnectFromPeripheral(peripheral.id);
        }

        void GetServices()
        {
            device.connection.GetServices();

            Debug.Log($"Getting Services for {peripheral.id}");
            instance.statusText.text = $"Getting Services for {peripheral.id}";
        }

        void GetCharacteristics(string service)
        {
            device.connection.GetCharacteristics(service);

            Debug.Log($"Getting Characteristics for {peripheral.id} {service}");
            instance.statusText.text = $"Getting Characteristics for {peripheral.id} {service}";
        }

        void SubscribeToCharacteristicUpdate(string service, string characteristic)
        {
            device.connection.SubscribeToCharacteristicUpdate(service, characteristic);

            Debug.Log($"Subscribing to {characteristic}");
            instance.statusText.text = $"Subscribing to {characteristic}";
        }

        void OnConnected(BluetoothConnection connection)
        {
            Debug.Log($"Connected to {connection.ID}");
            instance.statusText.text = $"Connected to {connection.ID}";

            connected = true;
            connecting = false;

            connection.OnData = OnData;
            connection.OnServices = OnServices;
            connection.OnCharacteristics = OnCharacteristics;
            device = new BluetoothDevice(peripheral.id, peripheral.name, peripheral.rssi, connection);

            GetServices();
        }

        void OnData(string id, byte[] data)
        {
            Debug.Log(Encoding.UTF8.GetString(data));
            instance.statusText.text = Encoding.UTF8.GetString(data);
        }

        void OnFailed(string id)
        {
            Debug.Log($"Faild to connect {id}");
            instance.statusText.text = $"Faild to connect {id}";

            connecting = false;
            connected = false;
        }

        void OnDisconnected(string id)
        {
            Debug.Log($"Disconnected from {id}");
            instance.statusText.text = $"Disconnected from {id}";

            connecting = false;
            connected = false;
        }

        void OnServices(string id, string[] services)
        {
            device.services = services;

            foreach (var service in services)
            {
                Debug.Log($"Service found - {service}");
                instance.statusText.text = $"Service found - {service}";

                if (service == SERVICE_UID)
                    GetCharacteristics(service);
            }
        }

        void OnCharacteristics(string id, string service, string[] characteristics)
        {
            foreach (var characteristic in characteristics)
            {
                Debug.Log($"Characteristic found - {characteristic}");
                instance.statusText.text = $"Characteristic found - {characteristic}";

                device.characteristics.Add(characteristic, service);

                if (characteristic == UART_TX_CHARACTERISTIC_UUID)
                {
                    SubscribeToCharacteristicUpdate(service, characteristic);
                }
            }

            StartCoroutine(PollData());
        }

        IEnumerator PollData()
        {
            yield return new WaitForSeconds(5);

            device.connection.Write(SERVICE_UID, UART_RX_CHARACTERISTIC_UUID, new PollRequestPacket().Serialize());

            Debug.Log($"Polling Data");
            instance.statusText.text = $"Polling Data";
        }
    }
}