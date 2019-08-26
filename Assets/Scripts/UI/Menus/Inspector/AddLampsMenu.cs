using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using VoyagerApp.Lamps;
using VoyagerApp.Utilities;
using VoyagerApp.Workspace;
using VoyagerApp.Workspace.Views;

namespace VoyagerApp.UI.Menus
{
    public class AddLampsMenu : Menu
    {
        [SerializeField] Transform container    = null;
        [SerializeField] AddLampItem prefab     = null;
        [SerializeField] Button addAllLampsBtn  = null;
        List<AddLampItem> items = new List<AddLampItem>();

        internal override void OnShow()
        {
            LampManager.instance.onLampAdded += OnLampAdded;
            WorkspaceManager.instance.onItemRemoved += ItemRemovedFromWorkspace;
            WorkspaceManager.instance.onItemAdded += ItemAddedToWorkspace;

            addAllLampsBtn.gameObject.SetActive(false);
            AddLampsToList();
        }

        internal override void OnHide()
        {
            LampManager.instance.onLampAdded -= OnLampAdded;
            WorkspaceManager.instance.onItemRemoved -= ItemRemovedFromWorkspace;

            foreach (var lamp in new List<AddLampItem>(items))
                RemoveLampItem(lamp);
        }

        void ItemRemovedFromWorkspace(WorkspaceItemView item)
        {
            if (item is LampItemView lampView)
                OnLampAdded(lampView.lamp);
        }

        void ItemAddedToWorkspace(WorkspaceItemView item)
        {
            if (item is LampItemView view)
            {
                var addItem = items.FirstOrDefault(v => v.lamp == view.lamp);
                if (addItem != null)
                    RemoveLampItem(addItem);
            }
        }

        void AddLampsToList()
        {
            var lamps = WorkspaceUtils.Lamps;
            foreach (var lamp in LampManager.instance.Lamps)
            {
                if (!lamps.Any(l => l.serial == lamp.serial))
                    OnLampAdded(lamp);
            }
        }

        public void AddAllLamps()
        {
            new List<AddLampItem>(items).ForEach(item => item.OnClick());
        }

        public void RemoveLampItem(AddLampItem item)
        {
            if (items.Contains(item))
            {
                items.Remove(item);
                Destroy(item.gameObject);
            }

            if (items.Count == 0)
                GetComponentInParent<MenuContainer>().ShowMenu(null);

            CheckForAddAllLampsButton();
        }

        void OnLampAdded(Lamp lamp)
        {
            if (!WorkspaceUtils.Lamps.Any(l => l == lamp))
            {
                AddLampItem item = Instantiate(prefab, container);
                item.SetLamp(lamp);
                items.Add(item);

                CheckForAddAllLampsButton();
            }
        }

        void CheckForAddAllLampsButton()
        {
            addAllLampsBtn.gameObject.SetActive(items.Count > 1);
        }
    }
}