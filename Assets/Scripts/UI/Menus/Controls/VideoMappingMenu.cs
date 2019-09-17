using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using VoyagerApp.Networking.Packages.Voyager;
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

        Video video;
        bool hasFpsInitialized;

        public void SetVideo(Video video)
        {
            this.video = video;
            if (hasFpsInitialized)
                fpsField.onChanged -= FpsChanged;
            SetupFps();
        }

        internal override void OnShow()
        {
            WorkspaceSelection.instance.onSelectionChanged += SelectionChanged;
            itshePick.onValueChanged.AddListener(ItsheChanged);
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
        }

        private void ItsheChanged(Itshe itshe)
        {
            var lamps = WorkspaceUtils.SelectedVoyagerLampItems;
            lamps.ForEach(_ => _.lamp.SetItshe(itshe));
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
            SceneManager.LoadScene(0);
        }

        void OnDestroy()
        {
            fpsField.onChanged -= FpsChanged;
        }
    }
}