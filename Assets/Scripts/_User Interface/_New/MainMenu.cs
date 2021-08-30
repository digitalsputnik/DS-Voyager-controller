using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using VoyagerController.Bluetooth;
using VoyagerController.Mapping;
using VoyagerController.ProjectManagement;
using VoyagerController.Serial;
using VoyagerController.Workspace;

namespace VoyagerController.UI
{
    public class MainMenu : Menu
    {
        private const string SELECT_ALL_TEXT = "SELECT ALL";
        private const string DESELECT_ALL_TEXT = "DESELECT ALL";

        [SerializeField] private GameObject _selectDeselectAllBtn = null;
        [SerializeField] private GameObject _alignmentBtn = null;
        [SerializeField] private GameObject _setup = null;

        [SerializeField] private Sprite _selectedAllIcon = null;
        [SerializeField] private Sprite _deselectAllIcon = null;

        private Text _selectDeselectText;
        private Image _selectDeselectIcon;

        public override void Start()
        {
            _selectDeselectText = _selectDeselectAllBtn.GetComponentInChildren<Text>();
            _selectDeselectIcon = _selectDeselectAllBtn.GetComponentsInChildren<Image>().FirstOrDefault(x => x.gameObject.name == "Icon");
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

        public void SelectDeselect()
        {
            if (!WorkspaceUtils.AllLampsSelected)
                WorkspaceUtils.SelectAllLamps();
            else
                WorkspaceUtils.DeselectAllLamps();
        }

        public void Alignment()
        {

        }

        public void Setup()
        {

        }

        public void EditEffectClick()
        {
            var item = WorkspaceSelection.GetSelected<VoyagerItem>().First();
            var meta = Metadata.Get<LampData>(item.LampHandle.Serial);
            EffectMapper.EnterEffectMapping(meta.Effect, true);
        }

        private void UpdateUserInterface()
        {
            var all = WorkspaceUtils.AllLampsSelected;

            _selectDeselectText.text = all ? DESELECT_ALL_TEXT : SELECT_ALL_TEXT;
            _selectDeselectIcon.sprite = all ? _deselectAllIcon : _selectedAllIcon;

            _selectDeselectAllBtn.SetActive(true);
            _alignmentBtn.SetActive(true);
            _setup.SetActive(true);
        }
    }
}
