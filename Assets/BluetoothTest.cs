using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DigitalSputnik.Bluetooth;
using UnityEngine;
using UnityEngine.UI;
using VoyagerApp.Networking.Voyager;
using VoyagerApp.UI;
using VoyagerApp.UI.Menus;

public class BluetoothTest : MonoBehaviour
{
    #region Singleton
    public static BluetoothTest instance;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else
            Destroy(this);
    }
    #endregion

    public List<BluetoothConnection> connected = new List<BluetoothConnection>();
    public List<PeripheralInfo> scannedItems = new List<PeripheralInfo>();
    public List<BLEItem> bleItems = new List<BLEItem>();

    public InspectorMenuContainer inspector = null;

    [SerializeField] Text infoText = null;
    [SerializeField] Transform bleContainer = null;
    [SerializeField] BLEItem prefab = null;
    [SerializeField] ClientModeMenu clientMenu = null;

    int idleTime = 30;
    public bool scanning = false;
    public bool connecting = false;
    public bool settingClient = false;

    void Start()
    {
        BluetoothHelper.Initialize(this, OnInitialized);
    }

    void OnInitialized()
    {
        Debug.Log("BluetoothLog: initialized bluetooth");
    }

    public static void UpdateInfoText(string text)
    {
        instance.infoText.text = text;
    }

    public void StartScanningBleLamps()
    {
        scanning = true;
        BluetoothHelper.StartScanningForLamps(OnScanned);
        StartCoroutine(LampsCheck());
    }

    public void StopScanningBleLamps()
    {
        scanning = false;
        BluetoothHelper.StopScanningForLamps();
    }

    public void ConnectToLamps()
    {
        connecting = true;
        BluetoothHelper.ConnectToPeripherals(this, instance.bleItems, OnConnected, OnFailed, OnDisconnected);
    }

    public void DisconnectFromAllLamps()
    {
        foreach (var item in bleItems.Where(l => l.connected).ToList())
            BluetoothAccess.Disconnect(item.id);
    }

    public void RemoveNotConnectedLamps()
    {
        foreach (var item in bleItems.Where(l => !l.connected).ToList())
        {
            RemoveItem(item.id);
        }
    }

    IEnumerator LampsCheck()
    {
        while (scanning)
        {
            yield return new WaitForSeconds(1);
            CheckForLamps();
        }
    }

    void CheckForLamps()
    {
        foreach (var item in bleItems.Where(l => !l.connected && !l.selected).ToList())
        {
            if (DateTime.Now.Subtract(item.lastScan).Seconds > idleTime)
                RemoveItem(item.id);
        }
    }

    void OnScanned(PeripheralInfo peripheral)
    {
        if (peripheral.name.Contains("-"))
        {
            string[] withArrow = peripheral.name.Split('-');
            peripheral.name = withArrow[0];
        }

        if (!instance.scannedItems.Any(i => i.id == peripheral.id) || instance.scannedItems.Count == 0)
        {
            AddItem(peripheral);
            Debug.Log($"BluetoothLog: scanned {peripheral.id}");
        }
        else if (bleItems.Any(l => l.id == peripheral.id))
        {
            UpdateItem(peripheral);
            Debug.Log($"BluetoothLog: updated {peripheral.id}");
        }
    }

    void OnConnected(BluetoothConnection connection)
    {
        BLEItem bleItem = instance.bleItems.FirstOrDefault(l => l.id == connection.ID) as BLEItem;
        bleItem.connected = true;
        connection.OnData = OnData;
        instance.connected.Add(connection);

        if(connected.Count == bleItems.Where(l => l.selected == true).Count() && connected.Count != 0)
        {
            OnAllSelectedLampsConnected();
        }

        Debug.Log($"BluetoothLog: Connected to {connection.ID}");
    }

    void OnAllSelectedLampsConnected()
    {
        settingClient = true;
        connecting = false;
        inspector.ShowMenu(clientMenu);
        clientMenu.SetupBluetooth(connected);
        StopScanningBleLamps();
        RemoveNotConnectedLamps();
        infoText.text = "Select lamps you wish to connect";
    }

    void OnData(string id, byte[] data)
    {
        Debug.Log($"BluetoothLog: Received from {id}: {Encoding.UTF8.GetString(data)}");
    }

    void OnFailed(string id)
    {
        Debug.Log($"BluetoothLog: Faild to connect {id}");
    }

    void OnDisconnected(string id)
    {
        Debug.Log($"BluetoothLog: Disconnected from {id}");
        if(!connecting) RemoveItem(id);
    }

    void RemoveItem(string id)
    {
        var scannedItem = scannedItems.FirstOrDefault(l => l.id == id);
        var bleItem = bleItems.FirstOrDefault(l => l.id == id);
        var connection = connected.FirstOrDefault(l => l.ID == id);

        if (scannedItem != null)
        {
            scannedItems.Remove(scannedItem);
            bleItems.Remove(bleItem);
            Destroy(bleItem.gameObject);
            if (connection != null)
                connected.Remove(connection);
        }
    }

    void AddItem(PeripheralInfo peripheral)
    {
        instance.scannedItems.Add(peripheral);

        BLEItem item = Instantiate(prefab, bleContainer);
        item.SetPeripheral(peripheral);
        instance.bleItems.Add(item);
    }

    void UpdateItem(PeripheralInfo peripheral)
    {
        BLEItem item = bleItems.FirstOrDefault(l => l.id == peripheral.id);
        item.UpdateRssi(peripheral.rssi.ToString());
        item.lastScan = DateTime.Now;
    }
}
