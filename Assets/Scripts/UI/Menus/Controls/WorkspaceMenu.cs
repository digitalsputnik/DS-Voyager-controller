using System.IO;
using UnityEngine;
using UnityEngine.UI;
using VoyagerApp.UI.Overlays;
using VoyagerApp.Utilities;
using VoyagerApp.Workspace;
using VoyagerApp.Workspace.Views;

namespace VoyagerApp.UI.Menus
{
    public class WorkspaceMenu : Menu
    {
        [SerializeField] GameObject infoText             = null;
        [SerializeField] GameObject splitter1            = null;
        [SerializeField] GameObject selectDeselectAllBtn = null;
        [SerializeField] GameObject selectColorFxBtn     = null;
        [SerializeField] GameObject splitter2            = null;
        [SerializeField] GameObject setColorFxBtn        = null;
        [SerializeField] GameObject editColorFxBtn       = null;
        [SerializeField] GameObject splitter3            = null;
        [SerializeField] GameObject setDmxBtn            = null;
        [SerializeField] GameObject splitter4            = null;
        [SerializeField] GameObject alignmentBtn         = null;

        Text selectDeselectText;

        public void AddPicture()
        {
            FileUtils.LoadPictureFromDevice(PicturePicked);
        }

        public void SelectDeselect()
        {
            if (!WorkspaceUtils.AllLampsSelected)
                WorkspaceUtils.SelectAll();
            else
                WorkspaceUtils.DeselectAll();
        }

        public void SelectWithSameEffect()
        {
            var effect = WorkspaceUtils.SelectedLamps[0].effect;
            WorkspaceUtils.SelectLampsWithEffect(effect);
        }

        public void EditEffectClick()
        {
            WorkspaceUtils.EnterToVideoMapping();
        }

        public override void Start()
        {
            selectDeselectText = selectDeselectAllBtn.GetComponentInChildren<Text>();
            DisableEnableItems();
            base.Start();
        }

        internal override void OnShow()
        {
            WorkspaceSelection.instance.onSelectionChanged += DisableEnableItems;
            WorkspaceManager.instance.onItemAdded += ItemAddedToWorkspace;
            WorkspaceManager.instance.onItemRemoved += ItemRemovedWorkspace;
            DisableEnableItems();
        }

        internal override void OnHide()
        {
            WorkspaceSelection.instance.onSelectionChanged -= DisableEnableItems;
            WorkspaceManager.instance.onItemAdded -= ItemAddedToWorkspace;
            WorkspaceManager.instance.onItemRemoved -= ItemRemovedWorkspace;
        }

        void ItemAddedToWorkspace(WorkspaceItemView item)
        {
            DisableEnableItems();
        }

        void ItemRemovedWorkspace(WorkspaceItemView item)
        {
            DisableEnableItems();
        }

        void DisableEnableItems()
        {
            bool one = WorkspaceUtils.AtLastOneLampSelected;
            bool all = WorkspaceUtils.AllLampsSelected;
            bool share = WorkspaceUtils.SelectedLampsHaveSameEffect;
            bool has = WorkspaceUtils.VoyagerLamps.Count != 0;

            infoText.SetActive(!one);

            splitter1.SetActive(one);
            selectDeselectAllBtn.SetActive(has);
            selectDeselectText.text = all ? "DESELECT ALL" : "SELECT ALL";
            selectColorFxBtn.SetActive(one && share);

            splitter2.SetActive(one);
            setColorFxBtn.SetActive(one);
            editColorFxBtn.SetActive(one && share);

            splitter3.SetActive(one);
            setDmxBtn.SetActive(one);

            splitter4.SetActive(one);
            alignmentBtn.SetActive(one);
        }

        void PicturePicked(string path)
        {
            if (path == null || path == "Null" || path == "") return;

            byte[] data = File.ReadAllBytes(path);

            Texture2D texture;

            if (Application.isMobilePlatform)
                texture = NativeGallery.LoadImageAtPath(path, - 1, false);
            else
            {
                texture = new Texture2D(2, 2);
                texture.LoadImage(data);
            }

            texture.Apply();

            WorkspaceManager.instance
                .InstantiateItem<PictureItemView>(texture)
                .PositionBasedCamera();
        }
    }
}
