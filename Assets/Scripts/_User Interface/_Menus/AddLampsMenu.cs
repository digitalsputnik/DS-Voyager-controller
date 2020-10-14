﻿using System.Collections.Generic;
using System.Linq;
using DigitalSputnik;
using DigitalSputnik.Voyager;
using UnityEngine;
using UnityEngine.UI;
using VoyagerController.Workspace;

namespace VoyagerController.UI
{
    public class AddLampsMenu : Menu
    {
        [SerializeField] private Transform _container = null;
        [SerializeField] private TextButton _addLampBtnPrefab = null;
        [SerializeField] private Button _addAllLampsBtn = null;

        private readonly List<TextButton> _lampItems = new List<TextButton>();
        
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
                if (LampValidToAdd(lamp))
                {
                    var title = lamp.Endpoint is BluetoothEndPoint ? "BT " + lamp.Serial : lamp.Serial;
                    var item = Instantiate(_addLampBtnPrefab, _container);
                    item.Setup(title, () => AddLampToWorkspace(lamp));
                    _lampItems.Add(item);
                }
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
                WorkspaceManager.InstantiateItem<VoyagerItem>(voyager);
        }

        private static bool LampValidToAdd(Lamp lamp)
        {
            return !WorkspaceContainsLamp(lamp) && LampConnected(lamp);
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
            // TODO: Implement!
            return true;
        }
        
        private void SubscribeEvents()
        {
            ApplicationManager.OnLampDiscovered += LampDiscovered;
            WorkspaceManager.OnItemAdded += WorkspaceChanged;
            WorkspaceManager.OnItemRemoved += WorkspaceChanged;
        }

        private void UnsubscribeEvents()
        {
            ApplicationManager.OnLampDiscovered -= LampDiscovered;
            WorkspaceManager.OnItemAdded -= WorkspaceChanged;
            WorkspaceManager.OnItemRemoved -= WorkspaceChanged;
        }

        private void LampDiscovered(Lamp lamp) => UpdateLampsList();
        private void WorkspaceChanged(WorkspaceItem item) => UpdateLampsList();
        
        public void AddAllLamps()
        {
            UnsubscribeEvents();
            foreach (var lampItem in _lampItems)
                lampItem.Click();
            UpdateLampsList();
            SubscribeEvents();
        }
    }
}