using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using VoyagerApp.Lamps;
using VoyagerApp.UI.Overlays;
using VoyagerApp.Utilities;
using VoyagerApp.Workspace;
using VoyagerApp.Workspace.Views;

namespace VoyagerApp.UI
{
    public class UpdatePrompt : MonoBehaviour
    {
        [SerializeField] MenuContainer container = null;
        [SerializeField] Menu updateMenu = null;
        [SerializeField] Button updateButton = null;
        [SerializeField] float waitTime = 5.0f;

        List<LampItemView> updatingLamps = new List<LampItemView>();

        private bool _showingLampOlder;
        private bool _showingLampNewer;

        private void Start()
        {
            WorkspaceManager.instance.onItemAdded += WorkspaceItemAdded;
        }

        private void OnDestroy()
        {
            WorkspaceManager.instance.onItemAdded -= WorkspaceItemAdded;
        }

        void WorkspaceItemAdded(WorkspaceItemView item)
        {
            var appVersion = new Version(UpdateSettings.VoyagerAnimationVersion);
            
            if (item is VoyagerItemView voyager)
            {
                var lampVersion = new Version(voyager.lamp.version);
                
                if (!voyager.lamp.updated && voyager.lamp.connected)
                {
                    StopCoroutine(WaitForAnothers());
                    StartCoroutine(WaitForAnothers());
                }
                else if (appVersion < lampVersion)
                {
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
                        WorkspaceSelection.instance.Clear();
                        foreach (var lampItem in WorkspaceUtils.LampItems)
                        {
                            if (!lampItem.lamp.updated && !updatingLamps.Any(l => l.lamp.serial == lampItem.lamp.serial))
                            {
                                WorkspaceSelection.instance.SelectItem(lampItem);
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
                new [] { "REMOVE", "OK" },
                new Action[] {
                    () =>
                    {
                        RemoveLampsWithNewerVersion();
                        _showingLampNewer = false;
                    }, null
                }
            );
        }

        private void RemoveNotUpdatedLamps()
        {
            var lampsNotUpdated = WorkspaceUtils.VoyagerLamps.Where(l => l.updated == false).ToList();

            WorkspaceSelection.instance.Clear();

            foreach (var lamp in lampsNotUpdated)
            {
                if (updatingLamps.All(l => l.lamp.serial != lamp.serial))
                {
                    var item = WorkspaceUtils.VoyagerItems.FirstOrDefault(v => v.lamp == lamp);
                    WorkspaceManager.instance.RemoveItem(item);
                }
            }
        }

        private void RemoveLampsWithNewerVersion()
        {
            var appVersion = new Version(UpdateSettings.VoyagerAnimationVersion);

            WorkspaceSelection.instance.Clear();
            
            foreach (var lamp in WorkspaceUtils.LampItems.ToArray())
            {
                var version = new Version(lamp.lamp.version[0], lamp.lamp.version[1]);
                if (version > appVersion)
                    WorkspaceManager.instance.RemoveItem(lamp);
            }
        }
    }
}