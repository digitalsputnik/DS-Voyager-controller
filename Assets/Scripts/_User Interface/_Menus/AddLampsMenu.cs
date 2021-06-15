using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DigitalSputnik;
using DigitalSputnik.Voyager;
using UnityEngine;
using UnityEngine.UI;
using VoyagerController.Bluetooth;
using VoyagerController.Effects;
using VoyagerController.ProjectManagement;
using VoyagerController.Workspace;

namespace VoyagerController.UI
{
    public class AddLampsMenu : Menu
    {
        private const double UPDATE_RATE = 0.5;
        
        [SerializeField] private Transform _container = null;
        [SerializeField] private AddLampItem _addLampBtnPrefab = null;
        [SerializeField] private Button _addAllLampsBtn = null;

        private readonly List<AddLampItem> _lampItems = new List<AddLampItem>();
        private readonly List<VoyagerLamp> _lampsInList = new List<VoyagerLamp>();
        private double _prevUpdate = 0.0;
        private bool addAllLampsClicked = false;
        private bool addAllLampsBleMaxed = false;

        internal override void OnShow()
        {
            _addAllLampsBtn.gameObject.SetActive(false);
            UpdateLampsList();
            SubscribeEvents();
        }

        private void Update()
        {
            if (TimeUtils.Epoch - _prevUpdate < UPDATE_RATE) return;
            if (LampStateChanged()) UpdateLampsList();
            _prevUpdate = TimeUtils.Epoch;
        }

        internal override void OnHide()
        {
            UnsubscribeEvents();
        }
        
        private bool LampStateChanged()
        {
            var lamps = LampManager.Instance.GetLampsOfType<VoyagerLamp>().Where(LampValidToAdd);
            return !lamps.All(_lampsInList.Contains);
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
                _lampsInList.Add(lamp);
            }
            
            UpdateAddAllLampsBtn();
        }
        
        private void ClearLampsList()
        {
            foreach (var lampItem in _lampItems)
                Destroy(lampItem.gameObject);
            _lampItems.Clear();
            _lampsInList.Clear();
        }

        private void UpdateAddAllLampsBtn()
        {
            _addAllLampsBtn.gameObject.SetActive(_lampItems.Count > 1);
        }
        
        private void AddLampToWorkspace(Lamp lamp)
        {
            if (lamp is VoyagerLamp voyager)
            {
                if (lamp.Endpoint is BluetoothEndPoint && !LessThanFiveBluetoothLampsOnWorkspace() && !addAllLampsClicked)
                {
                    DialogBox.Show(
                    "REMOVE BLE LAMPS",
                    "You can have a maximum on 5 bluetooth lamps in workspace at any time. " +
                    "Please remove a bluetooth lamp to add a new bluetooth lamp.",
                    new [] { "OK" },
                    new Action[] { null });
                    return;
                }
                else if(lamp.Endpoint is BluetoothEndPoint && !LessThanFiveBluetoothLampsOnWorkspace() && addAllLampsClicked && !addAllLampsBleMaxed)
                {
                    addAllLampsBleMaxed = true;
                    DialogBox.Show(
                    "REMOVE BLE LAMPS",
                    "You can have a maximum on 5 bluetooth lamps in workspace at any time. " +
                    "Please remove a bluetooth lamp to add a new bluetooth lamp.",
                    new[] { "OK" },
                    new Action[] { null });
                    return;
                }
                else if(lamp.Endpoint is BluetoothEndPoint && !LessThanFiveBluetoothLampsOnWorkspace() && addAllLampsClicked && addAllLampsBleMaxed)
                    return;

                var voyagerItem = WorkspaceManager.InstantiateItem<VoyagerItem>(voyager, WorkspaceUtils.PositionOfLastSelectedOrAddedLamp + new Vector3(0, -1.0f, 0), 1f, 0);
                
                if (voyager.Endpoint is LampNetworkEndPoint)
                    StartCoroutine(ApplyDefaultEffectAndColor(voyager));

                StartCoroutine(SelectAndSnapToLamp(voyagerItem));
                
                CloseMenuIfAllLampsAdded();

                Project.AutoSave();
            }
        }

        private static IEnumerator SelectAndSnapToLamp(WorkspaceItem voyager)
        {
            CameraMove.SetCameraPosition(voyager.transform.localPosition);
            yield return new WaitForFixedUpdate();
            WorkspaceSelection.Clear();
            WorkspaceSelection.SelectItem(voyager);
        }

        private static IEnumerator ApplyDefaultEffectAndColor(VoyagerLamp voyager)
        {
            if (!voyager.DmxModeEnabled)
            {
                LampEffectsWorker.ApplyItsheToVoyager(voyager, ApplicationSettings.AddedLampsDefaultColor);

                yield return new WaitForSeconds(0.5f);

                while (true)
                {
                    var effect = EffectManager.GetEffectWithName("white");

                    if (effect == null)
                    {
                        yield return new WaitForSeconds(0.2f);
                        continue;
                    }

                    ApplicationState.Playmode.Value = GlobalPlaymode.Play;

                    if (voyager.Endpoint is LampNetworkEndPoint)
                        LampEffectsWorker.ApplyEffectToLamp(voyager, effect);
                    break;
                }
            }
            
            var selected = WorkspaceSelection.GetSelected().ToList();
            WorkspaceSelection.Clear();
            selected.ForEach(WorkspaceSelection.SelectItem);
        }

        private static bool LampValidToAdd(Lamp lamp)
        {
            if (lamp.Endpoint is LampNetworkEndPoint)
                return !WorkspaceContainsLamp(lamp) && LampConnected(lamp);

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
                return voyager.Connected && !voyager.Passive && voyager.DmxPollReceived;
            
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
            StartCoroutine(AddAllLampsCoroutine());
        }

        private IEnumerator AddAllLampsCoroutine()
        {
            addAllLampsClicked = true;

            var addedLamps = new List<string>();

            UnsubscribeEvents();
            foreach (var lampItem in _lampItems.ToList())
            {
                yield return new WaitForFixedUpdate();
                lampItem.Click();
                addedLamps.Add(lampItem.Serial);
            }
            UpdateLampsList();
            SubscribeEvents();

            WorkspaceSelection.Clear();

            foreach (var lamp in WorkspaceManager.GetItems<VoyagerItem>().Where(l => addedLamps.Contains(l.LampHandle.Serial)).ToList())
            {
                yield return new WaitForFixedUpdate();
                WorkspaceSelection.SelectItem(lamp);
            }

            addAllLampsClicked = false;
            addAllLampsBleMaxed = false;

            CloseMenuIfAllLampsAdded();
        }

        private void CloseMenuIfAllLampsAdded()
        {
            if (!LampManager.Instance.GetLampsOfType<VoyagerLamp>().Any(LampValidToAdd))
                GetComponentInParent<MenuContainer>().ShowMenu(null);
        }
    }
}