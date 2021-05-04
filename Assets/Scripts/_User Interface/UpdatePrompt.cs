using DigitalSputnik;
using DigitalSputnik.Voyager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using VoyagerController.Workspace;

namespace VoyagerController.UI
{
    public class UpdatePrompt : MonoBehaviour
    {
        [SerializeField] MenuContainer container = null;
        [SerializeField] Menu updateMenu = null;
        [SerializeField] Button updateButton = null;
        [SerializeField] float waitTime = 5.0f;

        List<VoyagerItem> updatingLamps = new List<VoyagerItem>();
        public static List<string> newerWarnedLamps = new List<string>();

        private bool _showingLampOlder;
        private bool _showingLampNewer;

        private void Start()
        {
            WorkspaceManager.ItemAdded += WorkspaceItemAdded;
        }

        private void OnDestroy()
        {
            WorkspaceManager.ItemAdded -= WorkspaceItemAdded;
        }

        void WorkspaceItemAdded(WorkspaceItem item)
        {
            if (item is VoyagerItem voyager && voyager.LampHandle.Endpoint is LampNetworkEndPoint)
            {
                if (GetLampVersionStatus(voyager.LampHandle) == LampFirmwareVersionStatus.Older && voyager.LampHandle.Connected)
                {
                    StopCoroutine(WaitForAnothers());
                    StartCoroutine(WaitForAnothers());
                }
                else if (GetLampVersionStatus(voyager.LampHandle) == LampFirmwareVersionStatus.Newer && !newerWarnedLamps.Contains(voyager.LampHandle.Serial))
                {
                    newerWarnedLamps.Add(voyager.LampHandle.Serial);

                    StopCoroutine(WaitForAnothersLampNewer());
                    StartCoroutine(WaitForAnothersLampNewer());
                }
            }
        }

        private IEnumerator WaitForAnothers()
        {
            yield return new WaitForSeconds(waitTime);
            OnLampsOutdated();
        }

        private IEnumerator WaitForAnothersLampNewer()
        {
            yield return new WaitForSeconds(waitTime);
            OnLampNewer();
        }

        void OnLampsOutdated()
        {
            if (_showingLampOlder) return;

            _showingLampOlder = true;

            DialogBox.Show(
                "Update",
                "At least one outdated lamp found from network. " +
                "Would you like to update your lamps now?",
                new string[] { "REMOVE", "OK" },
                new Action[] {
                    () =>
                    {
                        RemoveNotUpdatedLamps();
                        _showingLampOlder = false;
                    },
                    () =>
                    {
                        WorkspaceSelection.Clear();
                        foreach (var lampItem in WorkspaceManager.GetItems<VoyagerItem>().Where(l => l.LampHandle.Endpoint is LampNetworkEndPoint).ToList())
                        {
                            if (GetLampVersionStatus(lampItem.LampHandle) == LampFirmwareVersionStatus.Older && 
                                !updatingLamps.Any(l => l.LampHandle.Serial == lampItem.LampHandle.Serial))
                            {
                                WorkspaceSelection.SelectItem(lampItem);
                                updatingLamps.Add(lampItem);
                            }
                        }
                        container.ShowMenu(updateMenu);
                        updateButton.onClick.Invoke();
                        _showingLampOlder = false;
                    }
                }
            );
        }

        private void OnLampNewer()
        {
            if (_showingLampNewer) return;

            _showingLampNewer = true;

            DialogBox.Show(
                "Lamp software is newer",
                "Workspace contains lamps with newer software than the app. Some features might not work. Update your application for better experience.",
                new[] { "REMOVE", "OK" },
                new Action[] {
                    () =>
                    {
                        RemoveLampsWithNewerVersion();
                        _showingLampNewer = false;
                    }, () => _showingLampNewer = false
                }
            );
        }

        private void RemoveNotUpdatedLamps()
        {
            var lampsNotUpdated = WorkspaceManager.GetItems<VoyagerItem>().Where(l => GetLampVersionStatus(l.LampHandle) == LampFirmwareVersionStatus.Older && 
                                                                                      l.LampHandle.Endpoint is LampNetworkEndPoint).ToList();
            WorkspaceSelection.Clear();

            foreach (var lamp in lampsNotUpdated)
            {
                if (updatingLamps.All(l => l.LampHandle.Serial != lamp.LampHandle.Serial))
                    WorkspaceManager.RemoveItem(lamp);
            }
        }

        private void RemoveLampsWithNewerVersion()
        {
            var newerLamps = WorkspaceManager.GetItems<VoyagerItem>().Where(l => GetLampVersionStatus(l.LampHandle) == LampFirmwareVersionStatus.Newer &&
                                                                                      l.LampHandle.Endpoint is LampNetworkEndPoint).ToList();
            WorkspaceSelection.Clear();

            foreach (var lamp in newerLamps)
                WorkspaceManager.RemoveItem(lamp);
        }

        LampFirmwareVersionStatus GetLampVersionStatus(VoyagerLamp lamp)
        {
            Version lampVersion = new Version(lamp.Version);
            Version softwareVersion = new Version(VoyagerUpdater.Version);

            if (lampVersion.CompareTo(softwareVersion) > 0)
                return LampFirmwareVersionStatus.Newer;
            else if (lampVersion.CompareTo(softwareVersion) < 0)
                return LampFirmwareVersionStatus.Older;
            else
                return LampFirmwareVersionStatus.Correct;
        }

        enum LampFirmwareVersionStatus
        {
            Older,
            Newer,
            Correct
        }
    }
}