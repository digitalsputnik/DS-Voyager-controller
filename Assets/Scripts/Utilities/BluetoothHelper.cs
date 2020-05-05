using System;
using System.Collections;
using System.Collections.Generic;
using DigitalSputnik.Bluetooth;
using UnityEngine;

public static class BluetoothHelper
{
    public const string SERVICE_UID = "6e400001-b5a3-f393-e0a9-e50e24dcca9e";

    static Dictionary<string, BluetoothConnectedHandler> _onConnect = new Dictionary<string, BluetoothConnectedHandler>();
    static Dictionary<string, Action<string>> _onFail = new Dictionary<string, Action<string>>();
    static Dictionary<string, Action<string>> _onDisconnect = new Dictionary<string, Action<string>>();

    public static bool IsInitialized => BluetoothAccess.IsInitialized;

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
        // Bluetooth access should check the initialization from OS,
        // but does not do it at the moment. That's why we wait.
        yield return new WaitForSeconds(0.5f);
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

    public static void ConnectToPeripheral(string id, BluetoothConnectedHandler onConnected, Action<string> onFail, Action<string> onDisconnect)
    {
        _onConnect[id] = onConnected;
        _onFail[id] = onFail;
        _onDisconnect[id] = onDisconnect;

        BluetoothAccess.Connect(id, OnConnect, OnFailed, OnDisconnect);
    }

    public static void DisconnectFromPeripheral(string id)
    {
        BluetoothAccess.Disconnect(id);
    }

    static void OnConnect(PeripheralAccess access)
    {
        _onConnect[access.ID]?.Invoke(new BluetoothConnection(access));
    }

    static void OnFailed(PeripheralInfo peripheral, string error)
    {
        _onFail[peripheral.id]?.Invoke(peripheral.id);
    }

    static void OnDisconnect(PeripheralInfo peripheral, string error)
    {
        _onDisconnect[peripheral.id]?.Invoke(peripheral.id);
    }
}

public delegate void BluetoothConnectedHandler(BluetoothConnection connection);

public class BluetoothConnection
{
    public Action<string[]> OnServices;
    public Action<string, string[]> OnCharacteristics;
    public Action<byte[]> OnData;
    public string ID => _access.ID;

    public PeripheralAccess _access;

    public BluetoothConnection(PeripheralAccess access)
    {
        _access = access;
    }

    public void GetServices()
    {
        _access.ScanServices(OnServicesUpdate);
    }

    public void GetCharacteristics(string service)
    {
        _access.ScanServiceCharacteristics(service, OnCharacteristicsUpdate);
    }

    public void SubscribeToCharacteristicUpdate(string service, string characteristic)
    {
        _access.SubscribeToCharacteristic(service, characteristic, OnDataUpdate);
    }

    public void Write(string service, string characteristic, byte[] data)
    {
        _access.WriteToCharacteristic(service, characteristic, data);
    }

    public void HandleDisconnection()
    {
        OnData = null;
        OnServices = null;
        OnCharacteristics = null;
    }

    void OnDataUpdate(PeripheralAccess access, string service, string characteristic, byte[] data)
    {
        OnData?.Invoke(data);
    }

    void OnServicesUpdate(PeripheralAccess access, string[] services)
    {
        OnServices?.Invoke(services);
    }

    void OnCharacteristicsUpdate(PeripheralAccess access, string service, string[] characteristics)
    {
        OnCharacteristics?.Invoke(service, characteristics);
    }
}