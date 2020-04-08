﻿using System.Collections.Generic;
using DigitalSputnik.Bluetooth;
using UnityEngine;
using UnityEngine.UI;
using VoyagerApp.UI.Menus;

public class BluetoothTest : MonoBehaviour
{
    public static BluetoothTest instance;

    [SerializeField] public BLEItem prefab;
    [SerializeField] public Transform container;
    [SerializeField] public Text statusText;

    public List<BLEItem> items = new List<BLEItem>();

    public List<PeripheralInfo> scannedDevices = new List<PeripheralInfo>();

    void Start()
    {
        BluetoothHelper.Initialize(this, OnInitialized);
    }

    void OnInitialized()
    {
        Debug.Log("BluetoothLog: initialized bluetooth");

        instance = this;

        StartScanningBleLamps();
    }

    public void StartScanningBleLamps()
    {
        Debug.Log($"BluetoothLog: Scanning Lamps");

        BluetoothHelper.StartScanningForLamps(OnScanned);
    }

    public void StopScanningBleLamps()
    {
        BluetoothHelper.StopScanningForLamps();
    }

    void OnScanned(PeripheralInfo peripheral)
    {
        Debug.Log($"BluetoothLog: Scanned Lamp - {peripheral.id} {peripheral.name} {peripheral.rssi}");

        BLEItem item = Instantiate(prefab, container);
        item.SetPeripheral(peripheral);
        items.Add(item);

        StopScanningBleLamps();
    }
}


