using DigitalSputnik.Bluetooth;
using System.Collections;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using VoyagerApp.Lamps.Voyager;

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
            if(instance.items.Where(i => i.connected == true).Count() < 5)
            {
                selected = !selected;

                var btn = GetComponent<Button>();
                ColorBlock btnColor = btn.colors;

                if (selected)
                {
                    btnColor.selectedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
                    btnColor.normalColor = new Color(0.8f, 0.8f, 0.8f, 1f);
                    Connect();
                }
                else
                {
                    btnColor.selectedColor = Color.white;
                    btnColor.normalColor = Color.white;
                    Disconnect();
                }

                btn.colors = btnColor;
            }
        }

        void Connect()
        {
            Log($"Connecting to lamp {peripheral.id}");
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

            Log($"Getting Services for {peripheral.id}");
        }

        void GetCharacteristics(string service)
        {
            device.connection.GetCharacteristics(service);

            Log($"Getting Characteristics for {peripheral.id} {service}");
        }

        void SubscribeToCharacteristicUpdate(string service, string characteristic)
        {
            device.connection.SubscribeToCharacteristicUpdate(service, characteristic);

            Log($"Subscribing to {characteristic}");
        }

        void OnConnected(BluetoothConnection connection)
        {
            Log($"Connected to {connection.ID}");

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
            Log(Encoding.UTF8.GetString(data));
        }

        void OnFailed(string id)
        {
            Log($"Faild to connect {id}");

            connecting = false;
            connected = false;
        }

        void OnDisconnected(string id)
        {
            Log($"Disconnected from {id}");

            connecting = false;
            connected = false;

            selected = false;

            var btn = GetComponent<Button>();
            ColorBlock btnColor = btn.colors;

            btnColor.selectedColor = Color.white;
            btnColor.normalColor = Color.white;

            btn.colors = btnColor;
        }

        void OnServices(string id, string[] services)
        {
            device.services = services;

            foreach (var service in services)
            {
                Log($"Service found - {service}");

                if (service == SERVICE_UID)
                    GetCharacteristics(service);
            }
        }

        void OnCharacteristics(string id, string service, string[] characteristics)
        {
            foreach (var characteristic in characteristics)
            {
                Log($"Characteristic found - {characteristic}");

                if (!device.characteristics.ContainsKey(characteristic))
                {
                    device.characteristics.Add(characteristic, service);
                }

                if (characteristic == UART_TX_CHARACTERISTIC_UUID)
                {
                    SubscribeToCharacteristicUpdate(service, characteristic);
                }
            }

            StartCoroutine(SetMaster());
        }

        IEnumerator SetMaster()
        {
            yield return new WaitForSeconds(5);

            VoyagerNetworkMode package;

            if (peripheral.name.Contains("-"))
            {
                string[] split = peripheral.name.Split('-');
                package = VoyagerNetworkMode.Master(split[0]);
            }
            else
            {
                package = VoyagerNetworkMode.Master(peripheral.name);
            }

            if (package != null)
            {
                var json = package.ToData();

                device.connection.Write(SERVICE_UID, UART_RX_CHARACTERISTIC_UUID, json);

                Log($"Polling Data");
            }
        }

        void Log(string log)
        {
            Debug.Log(log);
            instance.statusText.text = log;
        }
    }
}