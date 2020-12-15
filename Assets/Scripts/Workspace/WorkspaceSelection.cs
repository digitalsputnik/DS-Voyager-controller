using System.Collections.Generic;
using UnityEngine;
using VoyagerApp.Workspace.Views;

namespace VoyagerApp.Workspace
{
    public class WorkspaceSelection : MonoBehaviour
    {
        #region Singleton
        public static WorkspaceSelection instance;
        void Awake() => instance = this;
        #endregion

        public delegate void SelectionHandler();
        public event SelectionHandler onSelectionChanged;

        [SerializeField]
        List<ISelectableItem> selected = new List<ISelectableItem>();
        public List<ISelectableItem> Selected => selected;

        public void SelectItem(ISelectableItem selectable)
        {
            if (!Selected.Contains(selectable))
            {
                selectable.Select();
                selected.Add(selectable);
                onSelectionChanged?.Invoke();
            }
        }

        public bool Contains(ISelectableItem selectable)
        {
            return Selected.Contains(selectable);
        }

        public void ReselectItem()
        {
            onSelectionChanged?.Invoke();
        }

        public void DeselectItem(ISelectableItem selectable)
        {
            if (Selected.Contains(selectable))
            {
                selectable.Deselect();
                selected.Remove(selectable);
                onSelectionChanged?.Invoke();
            }
        }

        public void Clear()
        {
            selected.ForEach(s => s.Deselect());
            selected.Clear();
            onSelectionChanged?.Invoke();
        }

        void Start()
        {
            WorkspaceManager.instance.onItemRemoved += ItemRemovedFromWorkspace;
        }

        void OnDestroy()
        {
            WorkspaceManager.instance.onItemRemoved -= ItemRemovedFromWorkspace;
        }

        private void ItemRemovedFromWorkspace(WorkspaceItemView item)
        {
            if (item is ISelectableItem view)
            {
                if (selected.Contains(view))
                    selected.Remove(view);
            }
        }
    }
}