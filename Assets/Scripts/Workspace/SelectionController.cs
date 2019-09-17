using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoyagerApp.Utilities;
using VoyagerApp.Workspace;

namespace VoyagerApp.UI
{
    public class SelectionController : MonoBehaviour
    {
        SelectionControllerView selectionController;

        void Start()
        {
            WorkspaceSelection.instance.onSelectionChanged += SelectionChanged;
        }

        void SelectionChanged(WorkspaceSelection _)
        {
            if (WorkspaceUtils.SelectedLamps.Count > 1)
            {
                if (selectionController != null)
                    UpdateExisting();
                else
                    CreateNew();
            }
            else
            {
                if (selectionController != null)
                    Clear();
            }
        }

        void CreateNew()
        {
            Bounds bounds = new Bounds(
                WorkspaceUtils.SelectedVoyagerLampItems[0].transform.position,
                Vector3.zero);

            foreach (var item in WorkspaceUtils.SelectedVoyagerLampItems)
                bounds.Encapsulate(item.renderer.bounds);

            bounds.Expand(0.5f);

            selectionController = WorkspaceManager.instance.InstantiateItem<SelectionControllerView>(null);
            selectionController.SetBounds(bounds);

            foreach (var item in WorkspaceUtils.SelectedVoyagerLampItems)
                item.SetParent(selectionController);
        }

        void UpdateExisting()
        {
            Clear();
            CreateNew();
        }

        void Clear()
        {
            var children = selectionController.children;
            foreach (var item in new List<WorkspaceItemView>(children))
                item.SetParent(null);

            Destroy(selectionController.gameObject);
        }

        void OnDestroy()
        {
            WorkspaceSelection.instance.onSelectionChanged -= SelectionChanged;
        }
    }
}
