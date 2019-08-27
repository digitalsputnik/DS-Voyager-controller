using UnityEngine;
using UnityEngine.SceneManagement;
using VoyagerApp.Utilities;
using VoyagerApp.Workspace;

namespace VoyagerApp.UI.Menus
{
    public class DrawMenu : Menu
    {
        [SerializeField] GameObject infoText        = null;
        [SerializeField] GameObject videoMappingBtn = null;
        [SerializeField] ItshPickView itshPick      = null;
        [SerializeField] GameObject selectAllBtn    = null;
        [SerializeField] GameObject deselectAllBtn  = null;

        public void SelectAll()
        {
            foreach (var view in WorkspaceUtils.LampItems)
                WorkspaceSelection.instance.SelectLamp(view);
        }

        public void DeselectAll()
        {
            WorkspaceSelection.instance.Clear();
        }

        internal override void OnShow()
        {
            WorkspaceSelection.instance.onSelectionChanged += SelectionChanged;
            itshPick.onValueChanged.AddListener(ItshChanged);

            CheckForButtons();
        }

        internal override void OnHide()
        {
            WorkspaceSelection.instance.onSelectionChanged -= SelectionChanged;
            itshPick.onValueChanged.RemoveListener(ItshChanged);
        }

        void ItshChanged(Itsh itsh)
        {
            var lamps = WorkspaceUtils.SelectedVoyagerLampItems;
            lamps.ForEach(_ => _.lamp.SetItshWithoutVideo(itsh));
        }

        void SelectionChanged(WorkspaceSelection selection)
        {
            CheckForButtons();

            if (selection.Selected.Count == 1)
            {
                Itsh itsh = selection.Selected[0].lamp.itsh;
                if (itsh.Equals(default(Itsh))) itsh = Itsh.white;
                itshPick.Value = itsh;
            }
        }

        void CheckForButtons()
        {
            infoText.SetActive(!AtLeastOneSelected);
            videoMappingBtn.SetActive(AtLeastOneSelected);
            itshPick.gameObject.SetActive(AtLeastOneSelected);

            selectAllBtn.SetActive(!AllSelected);
            deselectAllBtn.SetActive(AtLeastOneSelected);
        }

        bool AllSelected
        {
            get => WorkspaceUtils.SelectedLamps.Count == WorkspaceUtils.Lamps.Count;
        }

        bool AtLeastOneSelected
        {
            get => WorkspaceSelection.instance.Selected.Count > 0;
        }

        public void VideoMappingBtnClicked()
        {
            var selected = WorkspaceSelection.instance.Selected.ToArray();
            var items = WorkspaceManager.instance.Items.ToArray();

            WorkspaceSaveLoad.Save("tmp/to_vm", selected);
            WorkspaceSaveLoad.Save("tmp/from_vm", items);

            SceneManager.LoadScene("Video Mapping");
        }
    }
}
