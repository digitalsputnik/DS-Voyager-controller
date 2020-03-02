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

    public List<PeripheralInfo> scannedItems = new List<PeripheralInfo>();

    public List<BLEItem> bleItems = new List<BLEItem>();

    public Text infoText = null;

    public InspectorMenuContainer inspector = null;

    [SerializeField] Transform bleContainer = null;
    [SerializeField] BLEItem prefab = null;
    [SerializeField] ClientModeMenu clientMenu = null;

    public List<BluetoothConnection> connected = new List<BluetoothConnection>();

    public int connectedCount = 0;

    void Start()
    {
        BluetoothHelper.Initialize(this, OnInitialized);
    }

    void OnInitialized()
    {
        Debug.Log("BluetoothLog: initialized bluetooth");
    }

    public void StartScanningBleLamps()
    {
        BluetoothHelper.StartScanningForLamps(OnScanned);
    }

    public void StopScanningBleLamps()
    {
        BluetoothHelper.StopScanningForLamps();
    }

    void OnScanned(PeripheralInfo peripheral)
    {
        if (!instance.scannedItems.Any(i => i.id == peripheral.id) || instance.scannedItems.Count == 0)
        {
            instance.scannedItems.Add(peripheral);

            InstantiateBleItem(peripheral);

            Debug.Log($"BluetoothLog: scanned {peripheral.id}");
        }
        else if(bleItems.Any(l => l.id == peripheral.id))
        {
            BLEItem item = bleItems.FirstOrDefault(l => l.id == peripheral.id);
            item.UpdateRssi(peripheral.rssi.ToString());
        }
    }

    void InstantiateBleItem(PeripheralInfo info)
    {
        BLEItem item = Instantiate(prefab, bleContainer);
        item.SetPeripheral(info);
        instance.bleItems.Add(item);
    }

    public void ConnectToLamps()
    {
        BluetoothHelper.ConnectToPeripherals(this, instance.bleItems, OnConnected, OnFailed, OnDisconnected);
    }

    void OnConnected(BluetoothConnection connection)
    {
        Debug.Log($"BluetoothLog: Connected to {connection.ID}");

        BLEItem bleItem = instance.bleItems.FirstOrDefault(l => l.id == connection.ID) as BLEItem;
        bleItem.connected = true;
        connection.OnData = OnData;
        instance.connected.Add(connection);
        connectedCount++;

        if(connectedCount == bleItems.Where(l => l.selected == true).Count())
        {
            infoText.text = "Select lamps you wish to connect";
            inspector.ShowMenu(clientMenu);
            clientMenu.SetupBluetooth(connected);
            StopScanningBleLamps();
        }
        
        connection.Write(new PollRequestPacket().Serialize());
    }

    public void ResetValues()
    {
        bleItems = new List<BLEItem>();
        scannedItems = new List<PeripheralInfo>();
        connected = new List<BluetoothConnection>();
        connectedCount = 0;
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
        var scannedItem = scannedItems.FirstOrDefault(l => l.id == id);
        var bleItem = bleItems.FirstOrDefault(l => l.id == id);
        var connection = connected.FirstOrDefault(l => l.ID == id);
        if (scannedItem != null)
        {
            connectedCount--;
            scannedItems.Remove(scannedItem);
            bleItems.Remove(bleItem);
            Destroy(bleItem.gameObject);
            if (connection != null)
                connected.Remove(connection);
        }
    }
}
