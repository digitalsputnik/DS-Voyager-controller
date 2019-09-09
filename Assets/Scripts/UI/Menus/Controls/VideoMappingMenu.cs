using UnityEngine;
using UnityEngine.SceneManagement;
using VoyagerApp.Networking.Packages.Voyager;
using VoyagerApp.Utilities;
using VoyagerApp.Videos;

namespace VoyagerApp.UI.Menus
{
    public class VideoMappingMenu : Menu
    {
        [SerializeField] IntField fpsField      = null;
        [SerializeField] VideoMapper mapper     = null;

        Video video;

        public void SetVideo(Video video)
        {
            this.video = video;
            SetupFps();
        }

        void SetupFps()
        {
            fpsField.SetValue((int)video.fps);
            fpsField.onChanged += FpsChanged;
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