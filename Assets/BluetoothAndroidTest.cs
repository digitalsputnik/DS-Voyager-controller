using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DigitalSputnik.Bluetooth;
using UnityEngine;
using VoyagerApp.Lamps.Voyager;
using VoyagerApp.Networking.Voyager;

public class BluetoothAndroidTest : MonoBehaviour
{
    IBluetoothInterfaceTest _interface;

    public static string UART_SERVICE_UID = "6e400001-b5a3-f393-e0a9-e50e24dcca9e";
    public static string UART_RX_CHARACTERISTIC_UUID = "6e400002-b5a3-f393-e0a9-e50e24dcca9e";
    public static string UART_TX_CHARACTERISTIC_UUID = "6e400003-b5a3-f393-e0a9-e50e24dcca9e";

    public string ssid = "";
    public string password = "";

    List<BluetoothDevice> devices = new List<BluetoothDevice>(); 

    void Start()
    {
        _interface = new AndroidBluetoothInterfaceTest();

        _interface.Initialize();

        StartCoroutine(StartScanningLamps());
    }

    IEnumerator StartScanningLamps()
    {
        yield return new WaitForSeconds(1);

        var services = new string[] { UART_SERVICE_UID };

        Debug.Log($"BluetoothLog: Scanning Lamps");

        _interface.StartScanning(services, OnScanned);
    }

    void StopScanningLamps()
    {
        Debug.Log($"BluetoothLog: Lamp Scanning Stopped");

        _interface.StopScanning();
    }

    void Connect(string id)
    {
        Debug.Log($"BluetoothLog: Connecting to lamp");

        SetCharacteristicsUpdateCallback();

        _interface.Connect(id, OnConnect, OnFail, OnDisconnect);
    }

    void Disconnect(string id)
    {
        Debug.Log($"BluetoothLog: Disconnecting from lamp");

        _interface.Disconnect(id);
    }

    void GetServices(string id)
    {
        Debug.Log($"BluetoothLog: Getting Services");

        _interface.GetServices(id, OnService);
    }

    void GetCharacteristic(string id, string service, string uuid)
    {
        Debug.Log($"BluetoothLog: Getting Characteristics for ID: {id} UUID: {uuid}");

        _interface.GetCharacteristic(id, service, uuid, OnCharacteristic);
    }

    void SubscribeToCharacteristic(string id, string characteristic)
    {
        Debug.Log($"BluetoothLog: Subscribing to characteristic");

        _interface.SubscribeToCharacteristicUpdate(id, characteristic);
    }

    void SetCharacteristicsUpdateCallback()
    {
        Debug.Log($"BluetoothLog: Setting characteristic update callback");

        _interface.SetCharacteristicsUpdateCallback(OnMessage);
    }

    void WriteToCharacteristic(string id, string characteristic, byte[] data)
    {
        Debug.Log($"BluetoothLog: Writing to characteristic");

        _interface.WriteToCharacteristic(id, characteristic, data);
    }

    void OnScanned(string id, string name, int rssi)
    {
        Debug.Log($"BluetoothLog: Lamp Scanned - ID: {id} Name: {name} Rssi {rssi}");

        BluetoothDevice device = new BluetoothDevice(id, name, rssi);
        devices.Add(device);

        StopScanningLamps();
        Connect(id);
    }

    void OnConnect(string id)
    {
        Debug.Log($"BluetoothLog: Lamp Connected - ID: {id}");

        var currentDevice = devices.FirstOrDefault(l => l.id == id);
        currentDevice.connected = true;

        GetServices(id);
    }

    void OnDisconnect(string id, string error)
    {
        Debug.Log($"BluetoothLog: Lamp Disconnected - ID: {id}");

        var currentDevice = devices.FirstOrDefault(l => l.id == id);
        currentDevice.connected = false;
    }

    void OnFail(string id, string error)
    {
        Debug.Log($"BluetoothLog: Lamp Connection Failed - ID: {id}");

        var currentDevice = devices.FirstOrDefault(l => l.id == id);
        currentDevice.connected = false;
    }

    void OnService(string id, string service)
    {
        Debug.Log($"BluetoothLog: Service Found - ID: {id} Service: {service}");

        var currentDevice = devices.FirstOrDefault(l => l.id == id);
        currentDevice.services.Add(service);

        GetCharacteristic(id, service, UART_RX_CHARACTERISTIC_UUID);
        GetCharacteristic(id, service, UART_TX_CHARACTERISTIC_UUID);
    }

    void OnCharacteristic(string id, string service, string characteristic)
    {
        Debug.Log($"BluetoothLog: Characteristic Found - ID: {id} Service: {service} Characteristic: {characteristic}");

        var currentDevice = devices.FirstOrDefault(l => l.id == id);
        currentDevice.characteristics.Add(characteristic, service);

        if (characteristic == UART_TX_CHARACTERISTIC_UUID)
            SubscribeToCharacteristic(id, characteristic);

        StartCoroutine(write());
    }

    void OnMessage(string id, string characteristic, int status, string message)
    {
        Debug.Log($"BluetoothLog: New Message - ID: {id} Characteristic: {characteristic} Message: {message}");
    }

    IEnumerator write()
    {
        yield return new WaitForSeconds(1);

        var package = VoyagerNetworkMode.Client(ssid, password, devices[0].name).ToData();

        string withoutOpCode = Encoding.UTF8.GetString(package, 0, package.Length);
        string withOpCode = @"{""op_code"": ""network_mode_request"", " + withoutOpCode.Substring(1);

        byte[] data = Encoding.UTF8.GetBytes(withOpCode);

        foreach(var characteristic in devices[0].characteristics)
        {
            if(characteristic.Key == UART_RX_CHARACTERISTIC_UUID)
            {
                Debug.Log($"BluetoothLog: Writing to Characteristic - Characteristic: {characteristic.Key}");

                //WriteToCharacteristic(devices[0].gatt, characteristic.GetCharacteristicObject(), data); missing active mode from request?

                WriteToCharacteristic(devices[0].id, characteristic.Key, new PollRequestPacket().Serialize());
            }
        }
    }
}


