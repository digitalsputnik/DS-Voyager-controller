using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VoyagerController.Workspace
{
    public class SelectionController : MonoBehaviour
    {
        private SelectionControllerItem _selectionController;

        public static Bounds Bounds;

        private void Start()
        {
            WorkspaceSelection.OnSelectionChanged += SelectionChanged;
        }
        
        private void OnDestroy()
        {
            WorkspaceSelection.OnSelectionChanged -= SelectionChanged;
        }

        private void SelectionChanged()
        {
            if (WorkspaceSelection.Selected.Any())
            {
                if (_selectionController != null)
                    UpdateExisting();
                else
                    CreateNew();
            }
            else
            {
                if (_selectionController != null)
                    Clear();
            }
        }

        private void CreateNew()
        {
            Bounds = new Bounds(
                WorkspaceSelection.Selected.First().SelectPositions[0],
                Vector3.zero);

            foreach (var item in WorkspaceSelection.Selected)
                Bounds.Encapsulate(item.Bounds);

            Bounds.Expand(0.5f);

            _selectionController = WorkspaceManager.InstantiateItem<SelectionControllerItem>(null);
            _selectionController.SetBounds(Bounds);

            foreach (var item in WorkspaceSelection.Selected)
                item.SetParent(_selectionController);
        }

        private void UpdateExisting()
        {
            Clear();
            CreateNew();
        }

        private void Clear()
        {
            var children = _selectionController.Children;
            foreach (var item in new List<WorkspaceItem>(children))
                item.SetParent(_selectionController.Parent);
            WorkspaceManager.RemoveItem(_selectionController);
        }
    }
}
