using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VoyagerApp.Lamps;
using VoyagerApp.Utilities;
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
            LampManager.instance.onLampBroadcasted += OnLampBroadcasted;
        }

        void OnDestroy()
        {
            WorkspaceManager.instance.onItemRemoved -= ItemRemovedFromWorkspace;
            LampManager.instance.onLampBroadcasted -= OnLampBroadcasted;
        }

        private void OnLampBroadcasted(Lamp lamp)
        {
            if (!WorkspaceUtils.VoyagerItems.Any(l => l.lamp == lamp))
                return;

            var vlamp = WorkspaceUtils.VoyagerItems.FirstOrDefault(l => l.lamp == lamp);

            if (vlamp == null)
                return;

            if (instance.selected.Count == 1 && instance.Selected.Contains(vlamp))
                return;

            instance.Clear();
            WorkspaceUtils.SetCameraPosition(vlamp.transform.localPosition);
            instance.SelectItem(vlamp);
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