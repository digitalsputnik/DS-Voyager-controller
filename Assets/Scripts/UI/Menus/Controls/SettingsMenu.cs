using System.IO;
using UnityEngine;
using VoyagerApp.Utilities;
using VoyagerApp.Workspace;
using VoyagerApp.Workspace.Views;

namespace VoyagerApp.UI.Menus
{
    public class SettingsMenu : Menu
    {
        [SerializeField] GameObject selectModeBtn = null;

        public override void Start()
        {
            base.Start();
            WorkspaceManager.instance.onItemAdded += WorkspaceChanged;
            WorkspaceManager.instance.onItemRemoved += WorkspaceChanged;
            WorkspaceChanged(null);
        }

        public void WorkspaceChanged(WorkspaceItemView item)
        {
            selectModeBtn.SetActive(WorkspaceUtils.Lamps.Count > 0);
        }

        public void AddPicture()
        {
            FileUtils.LoadPictureFromDevice(VideoPicked);
        }

        void VideoPicked(string path)
        {
            if (path == null || path == "Null" || path == "") return;

            byte[] data = File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(data);
            texture.Apply();

            WorkspaceManager.instance.InstantiateItem<PictureItemView>(texture);
        }
    }
}