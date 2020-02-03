using System;
using System.Collections;
using UnityEngine;
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
            if (container.current == updateMenu || showing) return;

            showing = true;

            DialogBox.Show(
                "Update",
                "Outdated lamp found from network. " +
                "Would you like to update your lamp now?",
                new string[] { "CANCEL", "OK" },
                new Action[] {
                    () =>
                    {
                        showing = false;
                    }, 
                    () =>
                    {
                        container.ShowMenu(updateMenu);
                        WorkspaceSelection.instance.Clear();
                        foreach (var lamp in WorkspaceUtils.LampItems)
                        {
                            if (!lamp.lamp.updated)
                                WorkspaceSelection.instance.SelectItem(lamp);
                        }
                        showing = false;
                    }
                }
            );
        }
    }
}