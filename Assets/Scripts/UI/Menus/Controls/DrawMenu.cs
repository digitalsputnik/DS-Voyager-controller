using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using VoyagerApp.Lamps;
using VoyagerApp.Utilities;
using VoyagerApp.Videos;
using VoyagerApp.Workspace;
using VoyagerApp.Workspace.Views;

namespace VoyagerApp.UI.Menus
{
    public class DrawMenu : Menu
    {
        [SerializeField] GameObject infoText        = null;
        [SerializeField] ItshPickView itshPick      = null;
        [SerializeField] GameObject setVideoBtn     = null;
        [SerializeField] GameObject videoMappingBtn = null;
        [SerializeField] GameObject splitter        = null;
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
            WorkspaceSelection.instance.Enabled = true;
            WorkspaceSelection.instance.ShowSelection = true;

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
            itshPick.gameObject.SetActive(AtLeastOneSelected);
            setVideoBtn.SetActive(AtLeastOneSelected);

            videoMappingBtn.SetActive(
                AtLeastOneSelected &&
                SelectedItemsHaveSameVideo
            );

            splitter.SetActive(AtLeastOneSelected);

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
            if (!SelectedItemsHaveSameVideo) return;

            var video = WorkspaceUtils.SelectedLamps[0].video;

            WorkspaceSaveLoad.Save(
                FileUtils.WorkspaceStatePath,
                WorkspaceManager.instance.Items.ToArray());

            var settings = new VideoMappingSettings(
                LampsWithVideo(video),
                video);

            settings.Save();

            SceneManager.LoadScene("Video Mapping");
        }

        static List<Lamp> LampsWithVideo(Video video)
        {
            return LampManager
                .instance
                .Lamps
                .Where(l => l.video == video)
                .ToList();
        }

        bool SelectedItemsHaveSameVideo
        {
            get
            {
                if (WorkspaceUtils.SelectedLamps.Count == 0) return false;
                Video video = WorkspaceUtils.SelectedLamps[0].video;
                return WorkspaceUtils.SelectedLamps.All(l => l.video == video);
            }
        }
    }
}
