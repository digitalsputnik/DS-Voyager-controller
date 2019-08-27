﻿using System.IO;
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
            FileUtils.LoadPictureFromDevice(VideoPicked);
        }

        internal override void OnShow()
        {
            WorkspaceSelection.instance.ShowSelection = false;
            WorkspaceSelection.instance.Enabled = false;
        }

        internal override void OnHide()
        {
            WorkspaceSelection.instance.ShowSelection = true;
            WorkspaceSelection.instance.Enabled = true;
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