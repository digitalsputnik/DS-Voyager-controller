using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using VoyagerController.Bluetooth;
using VoyagerController.Mapping;
using VoyagerController.Serial;
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
            FileUtils.LoadPictureFromDevice(PicturePicked, false);
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
            var effect = Metadata.Get<LampData>(serial).Effect;

            foreach (var item in WorkspaceUtils.GetItemsWithSameEffect(effect).ToList())
                WorkspaceSelection.SelectItem(item);
        }

        public void EditEffectClick()
        {
            var item = WorkspaceSelection.GetSelected<VoyagerItem>().First();
            var meta = Metadata.Get<LampData>(item.LampHandle.Serial);
            EffectMapper.EnterEffectMapping(meta.Effect, true);
        }

        private void UpdateUserInterface()
        {
            var selectedLamps = WorkspaceSelection.GetSelected<VoyagerItem>().ToList();
            var lampsInWorkspace = WorkspaceManager.GetItems<VoyagerItem>().ToList();
            
            var has = lampsInWorkspace.Any();
            var one = selectedLamps.Any();
            var all = WorkspaceUtils.AllLampsSelected;
            var share = SelectedLampsShareSameEffect();
            var dmx = selectedLamps.Any(l => l.LampHandle.DmxModeEnabled);
            var anySerial = selectedLamps.Any(l => l.LampHandle.Endpoint is SerialEndPoint);
            var anyBluetooth = selectedLamps.Any(l => l.LampHandle.Endpoint is BluetoothEndPoint);

            _infoText.SetActive(!one);
            
            _splitter1.SetActive(one);
            _selectDeselectAllBtn.SetActive(has);
            _selectDeselectText.text = all ? DESELECT_ALL_TEXT : SELECT_ALL_TEXT;
            _selectColorFxBtn.SetActive(one && share && !anyBluetooth);
            
            _splitter2.SetActive(one && !anySerial && !anyBluetooth);
            _setEffectBtn.SetActive(one && !anySerial && !anyBluetooth);
            _editEffectBtn.SetActive(one && share && !dmx && !anySerial && !anyBluetooth);

            _splitter3.SetActive(one && !anyBluetooth);
            _setDmxBtn.SetActive(one && !anyBluetooth);
            
            _splitter4.SetActive(one);
            _alignmentBtn.SetActive(one);
        }

        private static bool SelectedLampsShareSameEffect()
        {
            if (!WorkspaceSelection.GetSelected<VoyagerItem>().Any()) return false;
            
            var serial = WorkspaceSelection.GetSelected<VoyagerItem>().First().LampHandle.Serial;
            var effect = Metadata.Get<LampData>(serial).Effect;
            return WorkspaceSelection.GetSelected<VoyagerItem>().All(v =>
            {
                var ser = v.LampHandle.Serial;
                return Metadata.Get<LampData>(ser).Effect == effect;
            });
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

            WorkspaceManager.InstantiateItem<PictureItem>(texture);
        }
    }
}
