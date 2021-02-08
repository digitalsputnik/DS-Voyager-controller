using System;
using System.Collections.Generic;
using System.Linq;
using DigitalSputnik;
using DigitalSputnik.Ble;
using DigitalSputnik.Voyager;
using UnityEngine;
using UnityEngine.UI;
using VoyagerController.Bluetooth;
using VoyagerController.Effects;
using VoyagerController.Workspace;

namespace VoyagerController.UI
{
    public class AddLampsMenu : Menu
    {
        [SerializeField] private Transform _container = null;
        [SerializeField] private AddLampItem _addLampBtnPrefab = null;
        [SerializeField] private Button _addAllLampsBtn = null;

        private readonly List<AddLampItem> _lampItems = new List<AddLampItem>();
        
        internal override void OnShow()
        {
            _addAllLampsBtn.gameObject.SetActive(false);
            UpdateLampsList();
            SubscribeEvents();
        }

        internal override void OnHide()
        {
            UnsubscribeEvents();
        }

        private void UpdateLampsList()
        {
            ClearLampsList();

            foreach (var lamp in LampManager.Instance.GetLampsOfType<VoyagerLamp>())
            {
                if (!LampValidToAdd(lamp)) continue;
                
                var item = Instantiate(_addLampBtnPrefab, _container);
                item.Setup(lamp, () => AddLampToWorkspace(lamp));
                _lampItems.Add(item);
            }
            
            UpdateAddAllLampsBtn();
        }
        
        private void ClearLampsList()
        {
            foreach (var lampItem in _lampItems)
                Destroy(lampItem.gameObject);
            _lampItems.Clear();
        }

        private void UpdateAddAllLampsBtn()
        {
            _addAllLampsBtn.gameObject.SetActive(_lampItems.Count > 1);
        }
        
        private void AddLampToWorkspace(Lamp lamp)
        {
            if (lamp is VoyagerLamp voyager)
            {
                if (lamp.Endpoint is BluetoothEndPoint && !LessThanFiveBluetoothLampsOnWorkspace())
                {
                    DialogBox.Show(
                    "REMOVE BLE LAMPS",
                    "You can have a maximum on 5 bluetooth lamps in workspace at any time. " +
                    "Please remove a bluetooth lamp to add a new bluetooth lamp.",
                    new string[] { "OK" },
                    new Action[] { null });
                    return;
                }

                var voyagerItem = WorkspaceManager.InstantiateItem<VoyagerItem>(voyager, WorkspaceUtils.PositionOfLastSelectedOrAddedLamp + new Vector3(0, -1.0f, 0));
                CameraMove.SetCameraPosition(voyagerItem.transform.localPosition);
                WorkspaceSelection.Clear();
                WorkspaceSelection.SelectItem(voyagerItem);

                if (Metadata.Get(voyager.Serial).Effect == null)
                {
                    var effect = EffectManager.GetEffectWithName("white.mp4");
                    LampEffectsWorker.ApplyEffectToLamp(voyager, effect);
                }
                
                CloseMenuIfAllLampsAdded();
            }
        }

        private static bool LampValidToAdd(Lamp lamp)
        {
            if (lamp.Endpoint is LampNetworkEndPoint)
                return !WorkspaceContainsLamp(lamp) && LampConnected(lamp);
            else
                return !WorkspaceContainsLamp(lamp);
        }
        
        private static bool WorkspaceContainsLamp(Lamp lamp)
        {
            if (lamp is VoyagerLamp voyager)
                return WorkspaceManager
                    .GetItems<VoyagerItem>()
                    .Any(l => l.LampHandle == voyager);
            return false;
        }
        
        private static bool LampConnected(Lamp lamp)
        {
            if (lamp is VoyagerLamp voyager)
                return voyager.Connected && !voyager.Passive;
            
            return lamp.Connected;
        }

        private static bool LessThanFiveBluetoothLampsOnWorkspace()
        {
            return WorkspaceManager.GetItems<VoyagerItem>().Count(l => l.LampHandle.Endpoint is BluetoothEndPoint) < 5;
        }

        private void SubscribeEvents()
        {
            ApplicationManager.OnLampDiscovered += LampDiscovered;
            ApplicationManager.OnLampBroadcasted += LampBroadcasted;
            WorkspaceManager.ItemAdded += WorkspaceChanged;
            WorkspaceManager.ItemRemoved += WorkspaceChanged;
        }

        private void UnsubscribeEvents()
        {
            ApplicationManager.OnLampDiscovered -= LampDiscovered;
            ApplicationManager.OnLampBroadcasted -= LampBroadcasted;
            WorkspaceManager.ItemAdded -= WorkspaceChanged;
            WorkspaceManager.ItemRemoved -= WorkspaceChanged;
        }

        private void LampDiscovered(Lamp lamp) => UpdateLampsList();

        private void LampBroadcasted(Lamp lamp)
        {
            if (LampValidToAdd(lamp))
                AddLampToWorkspace(lamp);
        }

        private void WorkspaceChanged(WorkspaceItem item) => UpdateLampsList();
        
        public void AddAllLamps()
        {
            UnsubscribeEvents();
            foreach (var lampItem in _lampItems.ToList())
                lampItem.Click();
            UpdateLampsList();
            SubscribeEvents();

            WorkspaceSelection.Clear();

            foreach (var lamp in WorkspaceManager.GetItems<VoyagerItem>().ToList())
                WorkspaceSelection.SelectItem(lamp);
            
            CloseMenuIfAllLampsAdded();
        }

        private void CloseMenuIfAllLampsAdded()
        {
            if (!LampManager.Instance.GetLampsOfType<VoyagerLamp>().Any(LampValidToAdd))
                GetComponentInParent<MenuContainer>().ShowMenu(null);
        }
    }
}