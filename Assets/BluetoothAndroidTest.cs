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

    List<BluetoothDevice> devices = new List<BluetoothDevice>();

    public static string UART_SERVICE_UID = "6e400001-b5a3-f393-e0a9-e50e24dcca9e";
    public static string UART_RX_CHARACTERISTIC_UUID = "6e400002-b5a3-f393-e0a9-e50e24dcca9e";
    public static string UART_TX_CHARACTERISTIC_UUID = "6e400003-b5a3-f393-e0a9-e50e24dcca9e";

    public string ssid = "";
    public string password = "";

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

    void Connect(object device)
    {
        Debug.Log($"BluetoothLog: Connecting to lamp");

        SetCharacteristicsUpdateCallback();

        _interface.Connect(device, OnConnect, OnFail, OnDisconnect);
    }

    void Disconnect(object gatt)
    {
        Debug.Log($"BluetoothLog: Disconnecting from lamp");

        _interface.Disconnect(gatt);
    }

    void GetServices(object gatt)
    {
        Debug.Log($"BluetoothLog: Getting Services");

        _interface.GetServices(gatt, OnService);
    }

    void GetCharacteristic(string id, object service, string uuid)
    {
        Debug.Log($"BluetoothLog: Getting Characteristics for ID: {id} UUID: {uuid}");

        _interface.GetCharacteristic(id, service, uuid, OnCharacteristic);
    }

    void SubscribeToCharacteristic(object gatt, object characteristic)
    {
        Debug.Log($"BluetoothLog: Subscribing to characteristic");

        _interface.SubscribeToCharacteristicUpdate(gatt, characteristic);
    }

    void SetCharacteristicsUpdateCallback()
    {
        Debug.Log($"BluetoothLog: Setting characteristic update callback");

        _interface.SetCharacteristicsUpdateCallback(OnMessage);
    }

    void WriteToCharacteristic(object gatt, object characteristic, byte[] data)
    {
        Debug.Log($"BluetoothLog: Writing to characteristic");

        _interface.WriteToCharacteristic(gatt, characteristic, data);
    }

    void OnScanned(string id, string name, int rssi, object device)
    {
        Debug.Log($"BluetoothLog: Lamp Scanned - ID: {id} Name: {name} Rssi {rssi}");

        StopScanningLamps();
        BluetoothDevice newDevice = new BluetoothDevice(id, name, rssi, device);
        devices.Add(newDevice);
        Connect(device);
    }

    void OnConnect(string id, object gatt)
    {
        Debug.Log($"BluetoothLog: Lamp Connected - ID: {id}");

        BluetoothDevice currentDevice = devices.FirstOrDefault(l => l.id == id);
        currentDevice.gatt = gatt;
        GetServices(gatt);
    }

    void OnDisconnect(string id, string error)
    {
        Debug.Log($"BluetoothLog: Lamp Disconnected - ID: {id}");
    }

    void OnFail(string id, string error)
    {
        Debug.Log($"BluetoothLog: Lamp Disconnected - ID: {id}");
    }

    void OnService(string id, string serviceUuid, object service)
    {
        Debug.Log($"BluetoothLog: Service Found - ID: {id} Service: {serviceUuid}");

        BluetoothDevice currentDevice = devices.FirstOrDefault(l => l.id == id);
        Service newService = new Service(serviceUuid, service);
        currentDevice.services.Add(newService);
        GetCharacteristic(id, service, UART_TX_CHARACTERISTIC_UUID);
        GetCharacteristic(id, service, UART_RX_CHARACTERISTIC_UUID);
    }

    void OnCharacteristic(string id, string characteristicUuid, object characteristic)
    {
        Debug.Log($"BluetoothLog: Characteristic Found - ID: {id} characteristic: {characteristicUuid}");

        BluetoothDevice currentDevice = devices.FirstOrDefault(l => l.id == id);
        Characteristic newCharacteristic = new Characteristic(characteristicUuid, characteristic); 
        currentDevice.characteristics.Add(newCharacteristic);

        if (characteristicUuid == UART_TX_CHARACTERISTIC_UUID)
            SubscribeToCharacteristic(currentDevice.gatt, characteristic);

        StartCoroutine(write());
    }

    void OnMessage(string id, object characteristic, int status, string message)
    {
        Debug.Log($"BluetoothLog: New Message - ID: {id} Message: {message}");
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
            if(characteristic.characteristicUuid == UART_RX_CHARACTERISTIC_UUID)
            {
                Debug.Log($"BluetoothLog: Writing to Characteristic - Characteristic: {characteristic.GetCharacteristicUuid()}");

                //WriteToCharacteristic(devices[0].gatt, characteristic.GetCharacteristicObject(), data); missing active mode from request?

                WriteToCharacteristic(devices[0].gatt, characteristic.GetCharacteristic(), new PollRequestPacket().Serialize());
            }
        }
    }
}


