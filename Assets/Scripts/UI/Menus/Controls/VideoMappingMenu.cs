using UnityEngine;
using UnityEngine.SceneManagement;
using VoyagerApp.Utilities;
using VoyagerApp.Videos;
using VoyagerApp.Workspace;

namespace VoyagerApp.UI.Menus
{
    public class VideoMappingMenu : Menu
    {
        [SerializeField] ItshPickView itshPick  = null;
        [SerializeField] IntField intField      = null;
        [SerializeField] VideoMapper mapper     = null;

        public override void Start()
        {
            base.Start();
            itshPick.onValueChanged.AddListener(ItshChanged);
            intField.onChanged += FpsChanged;
            FpsChanged(intField.Value);
            ItshChanged(itshPick.Value);
        }

        private void ItshChanged(Itsh itsh)
        {
            WorkspaceUtils.Lamps.ForEach(lamp => lamp.SetItsh(itsh));
        }

        private void FpsChanged(int value)
        {
            mapper.SetFps(value);
        }

        public void ReturnToWorkspace()
        {
            SceneManager.LoadScene(0);
        }

        public void Save()
        {
            var items = WorkspaceManager.instance.Items.ToArray();
            WorkspaceSaveLoad.Save("tmp/mapping", items);
        }

        public void Load()
        {
            WorkspaceSaveLoad.Load("tmp/mapping");
        }

        void OnDestroy()
        {
            itshPick.onValueChanged.RemoveListener(ItshChanged);
            intField.onChanged -= FpsChanged;
        }
    }
}