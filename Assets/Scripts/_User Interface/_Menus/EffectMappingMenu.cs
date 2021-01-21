using UnityEngine;
using UnityEngine.UI;
using VoyagerController.Mapping;
using VoyagerController.Workspace;

namespace VoyagerController.UI
{
    public class EffectMappingMenu : Menu
    {
        private const string SELECT_ALL_TEXT = "SELECT ALL";
        private const string DESELECT_ALL_TEXT = "DESELECT ALL";
        
        [SerializeField] private GameObject _selectDeselectAllBtn = null;
        
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
        }

        internal override void OnHide()
        {
            WorkspaceSelection.SelectionChanged -= UpdateUserInterface;
        }

        public void SelectDeselect()
        {
            if (!WorkspaceUtils.AllLampsSelected)
                WorkspaceUtils.SelectAllLamps();
            else
                WorkspaceUtils.DeselectAllLamps();
        }
        
        public void ExitEffectMapping()
        {
            EffectMapper.LeaveEffectMapping();
        }
        
        private void UpdateUserInterface()
        {
            var all = WorkspaceUtils.AllLampsSelected;
            _selectDeselectText.text = all ? DESELECT_ALL_TEXT : SELECT_ALL_TEXT;
        }
    }
}