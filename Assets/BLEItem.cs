using DigitalSputnik.Bluetooth;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

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
        public bool writeReceived = false;
        public bool settingClient = false;

        AddLampsMenu instance;

        public void SetPeripheral(PeripheralInfo _peripheral, AddLampsMenu _instance)
        {
            peripheral = _peripheral;
            instance = _instance;

            serialText.text = peripheral.name;
            rssiText.text = peripheral.rssi.ToString();
        }

        public void OnClick()
        {
            if(instance.scannedLamps.Where(i => i.selected == true).Count() < 5)
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

        public void Connect()
        {
            if (!connected)
            {
                Debug.Log($"Connecting to lamp {peripheral.id}");
                connecting = true;
                BluetoothHelper.ConnectToPeripheral(peripheral.id, OnConnected, OnFailed, OnDisconnected);
            }
        }

        public void Disconnect()
        {
            if (connected)
            {
                Debug.Log($"Disconnecting to lamp {peripheral.id}");

                BluetoothHelper.DisconnectFromPeripheral(peripheral.id);
            }
        }

        void GetServices()
        {
            Debug.Log($"Getting Services for {peripheral.id}");

            device.connection.GetServices();
        }

        void GetCharacteristics(string service)
        {
            Debug.Log($"Getting Characteristics for {peripheral.id} {service}");

            device.connection.GetCharacteristics(service);
        }

        void SubscribeToCharacteristicUpdate(string service, string characteristic)
        {
            Debug.Log($"Subscribing to {characteristic}");

            device.connection.SubscribeToCharacteristicUpdate(service, characteristic);
        }

        void OnConnected(BluetoothConnection connection)
        {
            if (!connected)
            {
                Debug.Log($"Connected to {connection.ID}");

                if (connection.ID == peripheral.id)
                {

                    connection.OnData = OnData;
                    connection.OnServices = OnServices;
                    connection.OnCharacteristics = OnCharacteristics;
                    device = new BluetoothDevice(peripheral.id, peripheral.name, peripheral.rssi, connection);

                    GetServices();
                }
            }
        }

        void OnFailed(string id)
        {
            Debug.Log($"Failed to connect {id}");

            connecting = false;
            connected = false;

            if (settingClient)
            {
                Connect();
            }
        }

        void OnDisconnected(string id)
        {
            Debug.Log($"Disconnected from {id}");

            connecting = false;
            connected = false;

            if (settingClient)
            {
                Connect();
            }
            else
            {
                device.connection.HandleDisconnection();
            }
        }

        void OnServices(string[] services)
        {
            if(device.services == null)
            {
                device.services = services;

                foreach (var service in services)
                {
                    Debug.Log($"Service found - {service}");

                    if (service == SERVICE_UID)
                        GetCharacteristics(service);
                }
            }
        }

        void OnCharacteristics(string service, string[] characteristics)
        {
            if (!device.characteristics.ContainsValue(service))
            {
                foreach (var characteristic in characteristics)
                {
                    Debug.Log($"Characteristic found - {characteristic}");

                    if (!device.characteristics.ContainsKey(characteristic))
                    {
                        device.characteristics.Add(characteristic, service);
                    }

                    if (characteristic == UART_TX_CHARACTERISTIC_UUID)
                    {
                        SubscribeToCharacteristicUpdate(service, characteristic);
                    }

                    if (device.characteristics.ContainsKey(UART_TX_CHARACTERISTIC_UUID) && device.characteristics.ContainsKey(UART_RX_CHARACTERISTIC_UUID))
                    {
                        connected = true;
                        connecting = false;
                        Debug.Log($"Fully Connected - {peripheral.id}");
                    }
                }
            }
        }

        void OnData(byte[] data)
        {
            Debug.Log(Encoding.UTF8.GetString(data));

            if (Encoding.UTF8.GetString(data) == @"{""op_code"": ""ble_ack""}")
            {
                writeReceived = true;
            }
        }

        public void Write(byte[] data)
        {
            writeReceived = false;

            device.connection.Write(SERVICE_UID, UART_RX_CHARACTERISTIC_UUID, data);

            Debug.Log($"Writing Data");
        }
    }
}