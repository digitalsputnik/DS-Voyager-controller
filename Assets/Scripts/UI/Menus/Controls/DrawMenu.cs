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
        [SerializeField] GameObject settingsBtn     = null;
        [SerializeField] ItshPickView itshPick      = null;

        internal override void OnShow()
        {
            WorkspaceSelection.instance.Enabled = true;
            WorkspaceSelection.instance.ShowSelection = true;
            WorkspaceSelection.instance.onSelectionChanged += SelectionChanged;

            itshPick.onValueChanged.AddListener(ItshChanged);

            CheckForButtons();
        }

        internal override void OnHide()
        {
            WorkspaceSelection.instance.onSelectionChanged -= SelectionChanged;
            WorkspaceSelection.instance.Enabled = false;

            itshPick.onValueChanged.RemoveListener(ItshChanged);
        }

        public void OnBack()
        {
            WorkspaceSelection.instance.ShowSelection = false;
            WorkspaceSelection.instance.Clear();
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
            settingsBtn.SetActive(AtLeastOneSelected);
            itshPick.gameObject.SetActive(AtLeastOneSelected);
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
