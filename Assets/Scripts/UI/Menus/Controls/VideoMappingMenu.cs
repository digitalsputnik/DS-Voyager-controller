using UnityEngine;
using UnityEngine.SceneManagement;
using VoyagerApp.Networking.Packages.Voyager;
using VoyagerApp.UI.Overlays;
using VoyagerApp.Utilities;
using VoyagerApp.Videos;
using VoyagerApp.Workspace;

namespace VoyagerApp.UI.Menus
{
    public class VideoMappingMenu : Menu
    {
        [SerializeField] IntField fpsField      = null;
        [SerializeField] ItshPickView itshePick = null;
        [SerializeField] VideoMapper mapper     = null;
        [Space(4)]
        [SerializeField] GameObject splitter    = null;
        [Space(4)]
        [SerializeField] GameObject selectAllBtn    = null;
        [SerializeField] GameObject deselectAllBtn  = null;
        [Space(4)]
        [SerializeField] CheckForExistingFrames existingFrames = null;

        Video video;
        bool hasFpsInitialized;

        public void SetVideo(Video video)
        {
            this.video = video;

            if (hasFpsInitialized)
                fpsField.onChanged -= FpsChanged;

            if (video != null)
                SetupFps();
        }

        internal override void OnShow()
        {
            WorkspaceSelection.instance.onSelectionChanged += SelectionChanged;
            itshePick.onValueChanged.AddListener(ItsheChanged);
            CheckSelectButtons();
        }

        internal override void OnHide()
        {
            WorkspaceSelection.instance.onSelectionChanged -= SelectionChanged;
            itshePick.onValueChanged.RemoveListener(ItsheChanged);
        }

        private void SelectionChanged(WorkspaceSelection selection)
        {
            if (selection.Selected.Count == 1)
            {
                Itshe itshe = selection.Selected[0].lamp.itshe;
                if (itshe.Equals(default(Itshe))) itshe = Itshe.white;
                itshePick.Value = itshe;
            }

            CheckSelectButtons();
        }

        private void ItsheChanged(Itshe itshe)
        {
            var lamps = WorkspaceUtils.SelectedVoyagerLampItems;
            lamps.ForEach(lamp =>
            {
                lamp.lamp.SetItshe(itshe);
                lamp.lamp.buffer.ClearBuffer();
            });
        }

        void SetupFps()
        {
            fpsField.SetValue((int)video.fps);
            fpsField.onChanged += FpsChanged;
            hasFpsInitialized = true;
        }

        private void FpsChanged(int value)
        {
            video.fps = value;
            var packet = new SetFpsPacket(value);

            foreach (var lamp in WorkspaceUtils.Lamps)
                NetUtils.VoyagerClient.SendPacket(lamp, packet);

            mapper.SetFps(value);
        }

        public void ReturnToWorkspace()
        {
            if (existingFrames.allRendered)
                SceneManager.LoadScene(0);
            else
            {
                DialogBox.Show(
                    "ARE YOU SURE?",
                    "ALL FRAMES ARE NOT RENDERED, YET.",
                    "STAY", "EXIT",
                    () => { }, () => SceneManager.LoadScene(0)
                );
            }
        }

        void OnDestroy()
        {
            fpsField.onChanged -= FpsChanged;
        }

        public void SelectAll()
        {
            foreach (var view in WorkspaceUtils.LampItems)
                WorkspaceSelection.instance.SelectLamp(view);
        }

        public void DeselectAll()
        {
            WorkspaceSelection.instance.Clear();
        }

        void CheckSelectButtons()
        {
            selectAllBtn.SetActive(!AllSelected);
            deselectAllBtn.SetActive(AtLeastOneSelected);

            splitter.SetActive(
                selectAllBtn.activeInHierarchy ||
                deselectAllBtn.activeInHierarchy);
        }

        bool AllSelected => WorkspaceUtils.SelectedLamps.Count == WorkspaceUtils.Lamps.Count;
        bool AtLeastOneSelected => WorkspaceSelection.instance.Selected.Count > 0;
    }
}