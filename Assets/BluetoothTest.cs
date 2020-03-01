using System.Collections.Generic;
using System.Linq;
using System.Text;
using DigitalSputnik.Bluetooth;
using UnityEngine;
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

    [SerializeField] Transform bleContainer = null;
    [SerializeField] BLEItem prefab = null;
    [SerializeField] InspectorMenuContainer inspector = null;
    [SerializeField] ClientModeMenu clientMenu = null;

    public List<BluetoothConnection> connected = new List<BluetoothConnection>();

    public string connecting;

    public int connectedCount = 0;

    void Start()
    {
        BluetoothHelper.Initialize(this, OnInitialized);
    }

    void OnInitialized()
    {
        Debug.Log("initialized bluetooth");
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

            Debug.Log($"scanned {peripheral.id}");
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
        Debug.Log($"Connected to {connection.ID}");

        BLEItem bleItem = instance.bleItems.FirstOrDefault(l => l.id == connection.ID) as BLEItem;
        bleItem.connected = true;
        connection.OnData = OnData;
        instance.connected.Add(connection);
        connectedCount++;

        if(connectedCount == bleItems.Where(l => l.selected == true).Count())
        {
            inspector.ShowMenu(clientMenu);
            clientMenu.SetupBluetooth(connected);
            StopScanningBleLamps();
        }
        
        connection.Write(new PollRequestPacket().Serialize());
    }

    void OnData(string id, byte[] data)
    {
        Debug.Log($"Reveived from {id}: {Encoding.UTF8.GetString(data)}");
    }

    void OnFailed(string id)
    {
        Debug.Log($"Faild to connect {id}");
    }

    void OnDisconnected(string id)
    {
        Debug.Log($"Disconnected from {id}");
    }
}
