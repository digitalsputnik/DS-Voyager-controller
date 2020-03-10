using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
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
        [SerializeField] float waitTime = 5.0f;
        [SerializeField] Button updateButton = null;

        bool showing;
        List<string> promptedSerials = new List<string>();

        void Start()
        {
            WorkspaceManager.instance.onItemAdded += WorkspaceItemAdded;
        }

        void OnDestroy()
        {
            WorkspaceManager.instance.onItemAdded -= WorkspaceItemAdded;
        }

        void WorkspaceItemAdded(WorkspaceItemView item)
        {
            if (item is VoyagerItemView voyager)
            {
                if (!voyager.lamp.updated && voyager.lamp.connected)
                {
                    //promptedSerials.Add(voyager.lamp.serial);
                    StopAllCoroutines();
                    StartCoroutine(WaitForAnothers());
                }
            }
        }

        IEnumerator WaitForAnothers()
        {
            yield return new WaitForSeconds(waitTime);
            OnLampsOutdated();
        }

        void OnLampsOutdated()
        {
            if (container.current == updateMenu || showing) return;

            showing = true;

            DialogBox.Show(
                "Update",
                "Outdated lamp found from network. " +
                "Would you like to update your lamp now?",
                new string[] { "REMOVE", "OK" },
                new Action[] {
                    () =>
                    {
                        RemoveNotUpdatedLamps();
                        
                        showing = false;
                    },
                    () =>
                    {
                        WorkspaceSelection.instance.Clear();
                        foreach (var lamp in WorkspaceUtils.LampItems)
                        {
                            if (!lamp.lamp.updated)
                                WorkspaceSelection.instance.SelectItem(lamp);
                        }
                        Menus.LampSettingsMenu.isFromUpdatePrompt = true;
                        container.ShowMenu(updateMenu);
                        showing = false;
                    }
                }
            );
        }

        void RemoveNotUpdatedLamps()
        {
            var lampsNotUpdated = WorkspaceUtils.VoyagerLamps.Where(l => l.updated == false).ToList();

            WorkspaceSelection.instance.Clear();

            foreach (var lamp in lampsNotUpdated)
            {
                var item = WorkspaceUtils.VoyagerItems.FirstOrDefault(v => v.lamp == lamp);
                WorkspaceManager.instance.RemoveItem(item);
            }
        }
    }
}