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
        [SerializeField] Button updateButton = null;
        [SerializeField] float waitTime = 5.0f;

        List<LampItemView> updatingLamps = new List<LampItemView>();

        bool showing;

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
            if (showing) return;

            showing = true;

            DialogBox.Show(
                "Update",
                "At least one outdated lamp found from network. " +
                "Would you like to update your lamps now?",
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
                if (!updatingLamps.Any(l => l.lamp.serial == lamp.serial))
                {
                    var item = WorkspaceUtils.VoyagerItems.FirstOrDefault(v => v.lamp == lamp);
                    WorkspaceManager.instance.RemoveItem(item);
                }
            }
        }
    }
}