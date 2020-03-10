using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DigitalSputnik.Bluetooth;
using UnityEngine;
using UnityEngine.UI;
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
    public Queue<BLEItem> bleItemsToConnect = new Queue<BLEItem>();

    public InspectorMenuContainer inspector = null;

    [SerializeField] Text infoText = null;
    [SerializeField] Transform bleContainer = null;
    [SerializeField] BLEItem prefab = null;
    [SerializeField] ClientModeMenu clientMenu = null;

    public bool scanning = false;
    public bool connecting = false;
    public bool reconnecting = false;
    public bool settingClient = false;

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
        scanning = true;
        BluetoothHelper.StartScanningForLamps(OnScanned);
        StartCoroutine(LampsCheck());
    }

    public void StopScanningBleLamps()
    {
        scanning = false;
        BluetoothHelper.StopScanningForLamps();
        StopCoroutine(LampsCheck());
    }

    IEnumerator ConnectToLamps()
    {
        reconnecting = true;
        if(bleItemsToConnect.Count != 0)
        {
            foreach (var item in bleItemsToConnect.ToList())
            {
                yield return new WaitUntil(() => connecting == false);
                connecting = true;
                item.isConnecting = true;
                BLEItem bleItem = bleItemsToConnect.Dequeue();
                BluetoothHelper.ConnectToPeripheral(bleItem, OnConnected, OnFailed, OnDisconnected);
                Debug.Log($"BluetoothLog: Connecting to: {bleItem.serial}");
            }
        }
        reconnecting = false;
    }
    public bool AllLampsConnnected()
    {
        if (bleItems.Where(l => l.connected).Count() == bleItems.Where(l => l.selected).Count())
            return true;
        else
            return false;
    }

    public void DisconnectAndRemoveAllLamps()
    {
        foreach (var item in bleItems.Where(l => l.connected).ToList())
        {
            BluetoothAccess.Disconnect(item.id);
        }

        foreach (var item in bleItems.Where(l => l.isConnecting).ToList())
        {
            item.androidDevice.Call("stopConnecting");
        }

        foreach (var item in bleItems)
            Destroy(item.gameObject);

        scannedItems = new List<PeripheralInfo>();
        bleItems = new List<BLEItem>();
        bleItemsToConnect = new Queue<BLEItem>();

        scanning = false;
        connecting = false;
        settingClient = false;
        reconnecting = false;
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
        if(bleItems.Count > 0)
        {
            foreach (var item in bleItems.ToList())
            {
                if (item.selected && !item.connected && !item.isConnecting)
                {
                    if ((bleItems.Where(l => l.isConnecting).Count() +
                        bleItems.Where(l => l.connected).Count() +
                        bleItemsToConnect.Count) < 5)
                    {
                        bleItemsToConnect.Enqueue(item);
                    }
                }
                else if (!item.selected && item.connected)
                    item.androidDevice.Call("disconnect");
                else if (!item.selected && item.isConnecting)
                    item.androidDevice.Call("stopConnecting");
            }

            if (!reconnecting)
                StartCoroutine(ConnectToLamps());
        }
    }

    void OnScanned(PeripheralInfo peripheral)
    {
        if (peripheral.name.Contains("-"))
        {
            string[] withArrow = peripheral.name.Split('-');
            peripheral.name = withArrow[0];
        }

        if (peripheral.name.Contains("DS"))
        {
            if (!instance.scannedItems.Any(i => i.id == peripheral.id) || instance.scannedItems.Count == 0)
            {
                AddItem(peripheral);
                Debug.Log($"BluetoothLog: Scanned lamp - ID: {peripheral.id} Name: {peripheral.name} Rssi: {peripheral.rssi}");
            }
            else if (bleItems.Any(l => l.id == peripheral.id))
            {
                UpdateItem(peripheral);
            }
        }
    }

    void OnConnected(BluetoothConnection connection)
    {
        BLEItem bleItem = instance.bleItems.FirstOrDefault(l => l.id == connection.ID) as BLEItem;
        connecting = false;
        bleItem.isConnecting = false;
        connection.OnData = OnData;
        Debug.Log($"BluetoothLog: Connected to {connection.ID}");
    }

    public void OnAllSelectedLampsConnected()
    {
        settingClient = true;
        inspector.ShowMenu(clientMenu);
        clientMenu.SetupBluetooth();
        infoText.text = "Select lamps you wish to connect";
    }

    void OnData(string id, byte[] data)
    {
        Debug.Log($"BluetoothLog: Received from {id}: {data}");
    }

    void OnFailed(string id)
    {
        Debug.Log($"BluetoothLog: Faild to connect {id}");
    }

    void OnDisconnected(string id)
    {
        Debug.Log($"BluetoothLog: Disconnected from {id}");
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
        item.UpdateInfo(peripheral);
    }

    public static void UpdateInfoText(string text)
    {
        instance.infoText.text = text;
    }
}


