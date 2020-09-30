using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace VoyagerController.UI
{
    public class WorkspaceMenu : Menu
    {
        [SerializeField] private GameObject _infoText = null;
        [SerializeField] private GameObject _splitter1 = null;
        [SerializeField] private GameObject _selectDeselectAllBtn = null;
        [SerializeField] private GameObject _selectColorFxBtn = null;
        [SerializeField] private GameObject _splitter2 = null;
        [SerializeField] private GameObject _setEffectBtn = null;
        [SerializeField] private GameObject _editEffectBtn = null;
        [SerializeField] private GameObject _splitter3 = null;
        [SerializeField] private GameObject _setDmxBtn = null;
        [SerializeField] private GameObject _splitter4 = null;
        [SerializeField] private GameObject _alignmentBtn = null;

        private Text _selectDeselectText;
        
        public override void Start()
        {
            _selectDeselectText = _selectDeselectAllBtn.GetComponentInChildren<Text>();
            DisableEnableItems();
            base.Start();
        }

        internal override void OnShow()
        {
            /* 
            WorkspaceSelection.instance.onSelectionChanged += DisableEnableItems;
            WorkspaceManager.instance.onItemAdded += ItemAddedToWorkspace;
            WorkspaceManager.instance.onItemRemoved += ItemRemovedWorkspace;
            DisableEnableItems();
            */
        }

        internal override void OnHide()
        {
            /*
            WorkspaceSelection.instance.onSelectionChanged -= DisableEnableItems;
            WorkspaceManager.instance.onItemAdded -= ItemAddedToWorkspace;
            WorkspaceManager.instance.onItemRemoved -= ItemRemovedWorkspace;
            */
        }

        public void AddPicture()
        {
            // FileUtils.LoadPictureFromDevice(PicturePicked);
        }

        public void SelectDeselect()
        {
            /* 
            if (!WorkspaceUtils.AllLampsSelected)
                WorkspaceUtils.SelectAll();
            else
                WorkspaceUtils.DeselectAll();
            */
        }

        public void SelectWithSameEffect()
        {
            /* 
            var effect = WorkspaceUtils.SelectedLamps[0].effect;
            WorkspaceUtils.SelectLampsWithEffect(effect);
            */
        }

        public void EditEffectClick()
        {
            // WorkspaceUtils.EnterToVideoMapping();
        }

        /*
        void ItemAddedToWorkspace(WorkspaceItemView item)
        {
            DisableEnableItems();
        }

        void ItemRemovedWorkspace(WorkspaceItemView item)
        {
            DisableEnableItems();
        }
        */

        private void DisableEnableItems()
        {
            /*
            bool one = WorkspaceUtils.AtLastOneLampSelected;
            bool all = WorkspaceUtils.AllLampsSelected;
            bool share = WorkspaceUtils.SelectedLampsHaveSameEffect;
            bool has = WorkspaceUtils.VoyagerLamps.Count != 0;
            bool hasDmx = WorkspaceUtils.AnySelectedLampsAreDmx;

            infoText.SetActive(!one);

            splitter1.SetActive(one);
            selectDeselectAllBtn.SetActive(has);
            selectDeselectText.text = all ? "DESELECT ALL" : "SELECT ALL";
            selectColorFxBtn.SetActive(one && share);

            splitter2.SetActive(one);
            setColorFxBtn.SetActive(one);
            editColorFxBtn.SetActive(one && share && !hasDmx);

            splitter3.SetActive(one);
            setDmxBtn.SetActive(one);

            splitter4.SetActive(one);
            alignmentBtn.SetActive(one);
            */
        }

        private void PicturePicked(string path)
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

            /*
            WorkspaceManager.instance
                .InstantiateItem<PictureItemView>(texture)
                .PositionBasedCamera();
            */
        }
    }
}
