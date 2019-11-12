using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VoyagerApp.Networking.Voyager;
using VoyagerApp.Utilities;
using VoyagerApp.Videos;
using VoyagerApp.Workspace;

namespace VoyagerApp.UI.Menus
{
    public class VideoMappingMenu : Menu
    {
        [SerializeField] VideoMapper mapper         = null;
        [SerializeField] Text selectDeselectBtnText = null;
        [SerializeField] IntField fpsField          = null;
        [SerializeField] GameObject splitter        = null;
        [SerializeField] GameObject alignmentBtn    = null;

        Video video;
        bool hasFpsInitialized;

        public void SetEffect(Video video)
        {
            this.video = video;

            if (hasFpsInitialized)
                fpsField.onChanged -= FpsChanged;

            if (video != null)
                SetupFps();
        }

        public void SelectDeselect()
        {
            if (!WorkspaceUtils.AllLampsSelected)
                WorkspaceUtils.SelectAll();
            else
                WorkspaceUtils.DeselectAll();
        }

        internal override void OnShow()
        {
            WorkspaceSelection.instance.onSelectionChanged += EnableDisableObjects;
            EnableDisableObjects();
        }

        internal override void OnHide()
        {
            WorkspaceSelection.instance.onSelectionChanged -= EnableDisableObjects;
        }

        void SetupFps()
        {
            fpsField.SetValue(video.fps);
            fpsField.onChanged += FpsChanged;
            hasFpsInitialized = true;
        }

        private void FpsChanged(int value)
        {
            video.fps = value;
            var packet = new SetFpsPacket(value);

            foreach (var lamp in WorkspaceUtils.Lamps)
                NetUtils.VoyagerClient.KeepSendingPacket(lamp, "set_fps", packet, VoyagerClient.PORT_SETTINGS, TimeUtils.Epoch);

            mapper.SetFps(value);
        }

        public void ReturnToWorkspace()
        {
            PlayerPrefs.SetInt("from_video_mapping", 1);
            SceneManager.LoadScene(0);
        }

        void OnDestroy()
        {
            fpsField.onChanged -= FpsChanged;
        }

        void EnableDisableObjects()
        {
            bool one = WorkspaceUtils.AtLastOneLampSelected;
            bool all = WorkspaceUtils.AllLampsSelected;

            selectDeselectBtnText.text = all ? "DESELECT ALL" : "SELECT ALL";
            splitter.SetActive(one);
            alignmentBtn.SetActive(one);
        }
    }
}