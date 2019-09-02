using UnityEngine;
using UnityEngine.SceneManagement;
using VoyagerApp.Videos;

namespace VoyagerApp.UI.Menus
{
    public class VideoMappingMenu : Menu
    {
        [SerializeField] IntField intField      = null;
        [SerializeField] VideoMapper mapper     = null;

        public override void Start()
        {
            base.Start();

            intField.onChanged += FpsChanged;
            //FpsChanged(intField.Value);
        }

        private void FpsChanged(int value)
        {
            //mapper.SetFps(value);
        }

        public void ReturnToWorkspace()
        {
            SceneManager.LoadScene(0);
        }

        void OnDestroy()
        {
            intField.onChanged -= FpsChanged;
        }
    }
}