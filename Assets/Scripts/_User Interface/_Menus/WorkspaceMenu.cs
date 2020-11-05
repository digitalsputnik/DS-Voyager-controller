using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using VoyagerController.Mapping;
using VoyagerController.Workspace;

namespace VoyagerController.UI
{
    public class WorkspaceMenu : Menu
    {
        private const string SELECT_ALL_TEXT = "SELECT ALL";
        private const string DESELECT_ALL_TEXT = "DESELECT ALL";
        
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
            UpdateUserInterface();
            base.Start();
        }

        internal override void OnShow()
        {
            WorkspaceSelection.SelectionChanged += UpdateUserInterface;
            WorkspaceManager.ItemAdded += OnItemEvent;
            WorkspaceManager.ItemRemoved += OnItemEvent;
            UpdateUserInterface();
        }

        private void OnItemEvent(WorkspaceItem item) => UpdateUserInterface();

        internal override void OnHide()
        {
            WorkspaceSelection.SelectionChanged -= UpdateUserInterface;
            WorkspaceManager.ItemAdded -= OnItemEvent;
            WorkspaceManager.ItemRemoved -= OnItemEvent;
        }

        public void AddPicture()
        {
            // FileUtils.LoadPictureFromDevice(PicturePicked);
        }

        public void SelectDeselect()
        {
            if (!WorkspaceUtils.AllLampsSelected)
                WorkspaceUtils.SelectAllLamps();
            else
                WorkspaceUtils.DeselectAllLamps();
        }

        public void SelectWithSameEffect()
        {
            var serial = WorkspaceSelection.GetSelected<VoyagerItem>().First().LampHandle.Serial;
            var effect = Metadata.Get(serial).Effect;

            foreach (var item in WorkspaceUtils.GetItemsWithSameEffect(effect))
                WorkspaceSelection.SelectItem(item);
        }

        public void EditEffectClick()
        {
            var item = WorkspaceSelection.GetSelected<VoyagerItem>().First();
            var meta = Metadata.Get(item.LampHandle.Serial);
            EffectMapper.EnterEffectMapping(meta.Effect);
        }

        private void UpdateUserInterface()
        {
            var selectedLamps = WorkspaceSelection.GetSelected<VoyagerItem>().ToList();
            var lampsInWorkspace = WorkspaceManager.GetItems<VoyagerItem>().ToList();
            
            var has = lampsInWorkspace.Any();
            var one = selectedLamps.Any();
            var all = WorkspaceUtils.AllLampsSelected;
            // TODO: Check if effects are shared
            // TODO: Check if any lamp is in DMX mode
            var share = SelectedLampsShareSameEffect();
            var dmx = false;
            
            _infoText.SetActive(!one);
            
            _splitter1.SetActive(one);
            _selectDeselectAllBtn.SetActive(has);
            _selectDeselectText.text = all ? DESELECT_ALL_TEXT : SELECT_ALL_TEXT;
            _selectColorFxBtn.SetActive(one && share);
            
            _splitter2.SetActive(one);
            _setEffectBtn.SetActive(one);
            _editEffectBtn.SetActive(one && share && !dmx);
            
            _splitter3.SetActive(one);
            _setDmxBtn.SetActive(one);
            
            _splitter4.SetActive(one);
            _alignmentBtn.SetActive(one);
        }

        private static bool SelectedLampsShareSameEffect()
        {
            if (!WorkspaceSelection.GetSelected().Any()) return false;
            
            var serial = WorkspaceSelection.GetSelected<VoyagerItem>().First().LampHandle.Serial;
            var effect = Metadata.Get(serial).Effect;
            return WorkspaceSelection.GetSelected<VoyagerItem>().All(v =>
            {
                var ser = v.LampHandle.Serial;
                return Metadata.Get(ser).Effect == effect;
            });
        }

        /* 
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

            WorkspaceManager.instance
                .InstantiateItem<PictureItemView>(texture)
                .PositionBasedCamera();
        }
        */
    }
}
