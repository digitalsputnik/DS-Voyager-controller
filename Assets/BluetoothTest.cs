using System.Text;
using DigitalSputnik.Bluetooth;
using UnityEngine;
using VoyagerApp.Networking.Voyager;

public class BluetoothTest : MonoBehaviour
{
    string connecting;

    void Start()
    {
        BluetoothHelper.Initialize(this, OnInitialized);
    }

    void OnInitialized()
    {
        Debug.Log("initialized bluetooth");

        BluetoothHelper.StartScanningForLamps(OnScanned);
    }

    void OnScanned(PeripheralInfo peripheral)
    {
        Debug.Log($"scanned {peripheral.id}");

        connecting = peripheral.id;
        BluetoothHelper.StopScanningForLamps();
        BluetoothHelper.ConnectToPeripheral(peripheral, OnConnected, OnFailed, OnDisconnected);
    }

    void OnConnected(BluetoothConnection connection)
    {
        Debug.Log($"Connected to {connection.ID}");

        connection.OnData = OnData;
        connection.Write(new PollRequestPacket().Serialize());
    }

    void OnData(byte[] data)
    {
        Debug.Log($"Reveived from {connecting}: {Encoding.UTF8.GetString(data)}");
    }

    void OnFailed()
    {
        Debug.Log($"Faild to connect {connecting}");
    }

    void OnDisconnected()
    {
        Debug.Log($"Disconnected from {connecting}");
    }
}
