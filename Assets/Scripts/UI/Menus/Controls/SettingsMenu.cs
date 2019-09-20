using System.IO;
using UnityEngine;
using VoyagerApp.Utilities;
using VoyagerApp.Workspace;
using VoyagerApp.Workspace.Views;

namespace VoyagerApp.UI.Menus
{
    public class SettingsMenu : Menu
    {
        public void AddPicture()
        {
            FileUtils.LoadPictureFromDevice(PicturePicked);
        }

        void PicturePicked(string path)
        {
            if (path == null || path == "Null" || path == "") return;

            byte[] data = File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(data);
            texture.Apply();

            WorkspaceManager.instance
                .InstantiateItem<PictureItemView>(texture)
                .PositionBasedCamera();
        }
    }
}