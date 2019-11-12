using System.Collections.Generic;
using UnityEngine;
using VoyagerApp.Utilities;
using VoyagerApp.Workspace;

namespace VoyagerApp.UI
{
    public class SelectionController : MonoBehaviour
    {
        SelectionControllerView selectionController;

        public static Bounds bounds;

        void Start()
        {
            WorkspaceSelection.instance.onSelectionChanged += SelectionChanged;
        }

        void SelectionChanged()
        {
            if (WorkspaceUtils.SelectedItems.Count > 0)
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
            bounds = new Bounds(
                WorkspaceUtils.SelectedItems[0].SelectPositions[0],
                Vector3.zero);

            foreach (var item in WorkspaceUtils.SelectedItems)
                bounds.Encapsulate(item.Bounds);

            bounds.Expand(0.5f);

            selectionController = WorkspaceManager.instance.InstantiateItem<SelectionControllerView>(null);
            selectionController.SetBounds(bounds);

            foreach (var item in WorkspaceUtils.SelectedItems)
                item.View.SetParent(selectionController);
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
                item.SetParent(selectionController.parent);
            WorkspaceManager.instance.RemoveItem(selectionController);
        }

        void OnDestroy()
        {
            WorkspaceSelection.instance.onSelectionChanged -= SelectionChanged;
        }
    }
}
