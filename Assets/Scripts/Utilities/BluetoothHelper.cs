using System;
using System.Collections;
using DigitalSputnik.Bluetooth;
using UnityEngine;

public static class BluetoothHelper
{
    public const string SERVICE_UID = "6e400001-b5a3-f393-e0a9-e50e24dcca9e";

    static BluetoothConnectedHandler _onConnect;
    static Action _onFail;
    static Action _onDisconnect;

    static BluetoothConnection _connection;

    public static void Initialize(MonoBehaviour behaviour, Action onInitialized)
    {
        if (Application.isMobilePlatform && !BluetoothAccess.IsInitialized)
        {
            BluetoothAccess.Initialize();
            behaviour.StartCoroutine(CoroutineWaitUntilBluetoothInitialized(onInitialized));
        }
    }

    static IEnumerator CoroutineWaitUntilBluetoothInitialized(Action onInitialized)
    {
        yield return new WaitUntil(() => BluetoothAccess.IsInitialized);
        onInitialized?.Invoke();
    }

    public static void StartScanningForLamps(PeripheralHandler onScanned)
    {
        var services = new string[] { "6e400001-b5a3-f393-e0a9-e50e24dcca9e" };
        BluetoothAccess.StartScanning(onScanned, services);
    }

    public static void StopScanningForLamps()
    {
        BluetoothAccess.StopScanning();
    }

    public static void ConnectToPeripheral(PeripheralInfo peripheral, BluetoothConnectedHandler onConnected, Action onFail, Action onDisconnect)
    {
        _onConnect = onConnected;
        _onFail = onFail;
        _onDisconnect = onDisconnect;

        BluetoothAccess.Connect(peripheral.id, OnConnect, OnFailed, OnDisconnect);
    }

    static void OnConnect(PeripheralAccess access)
    {
        _connection = new BluetoothConnection(access);
        _onConnect?.Invoke(_connection);
    }

    static void OnFailed(PeripheralInfo peripheral, string error)
    {
        _onFail?.Invoke();
    }

    static void OnDisconnect(PeripheralInfo peripheral, string error)
    {
        if (_connection != null)
        {
            _connection.HandleDisconnection();
            _connection = null;
        }

        _onDisconnect?.Invoke();
    }
}

public delegate void BluetoothConnectedHandler(BluetoothConnection connection);

public class BluetoothConnection
{
    const string CHARACTERISTICS = "6e400002-b5a3-f393-e0a9-e50e24dcca9e";

    public Action<byte[]> OnData;
    public string ID => _access.ID;

    PeripheralAccess _access;

    public BluetoothConnection(PeripheralAccess access)
    {
        _access = access;
        _access.SubscrubeToCharacteristic(BluetoothHelper.SERVICE_UID, CHARACTERISTICS, OnDataUpdate);
    }

    public void Write(byte[] data)
    {
        _access.WriteToCharacteristic(BluetoothHelper.SERVICE_UID, CHARACTERISTICS, data);
    }

    public void HandleDisconnection()
    {
        OnData = null;
    }

    void OnDataUpdate(PeripheralAccess access, string service, string characteristic, byte[] data)
    {
        OnData?.Invoke(data);
    }
}