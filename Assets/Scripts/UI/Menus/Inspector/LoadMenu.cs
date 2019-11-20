using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VoyagerApp.Lamps;
using VoyagerApp.Projects;
using VoyagerApp.UI.Overlays;
using VoyagerApp.Utilities;
using VoyagerApp.Videos;

namespace VoyagerApp.UI.Menus
{
    public class LoadMenu : Menu
    {
        [SerializeField] LoadMenuItem itemPrefab = null;
        [SerializeField] Transform container     = null;

        List<LoadMenuItem> items = new List<LoadMenuItem>();
        ProjectSaveData data;

        internal override void OnShow()
        {
            ClearOldItems();
            DisplayAllItems();
        }

        void ClearOldItems()
        {
            new List<LoadMenuItem>(items).ForEach(RemoveItem);
        }

        public void RemoveItem(LoadMenuItem item)
        {
            items.Remove(item);
            if (item.gameObject != null)
                Destroy(item.gameObject);
        }

        public void Import()
        {
            FileUtils.LoadProject(OnImportFile);
        }

        void OnImportFile(string file)
        {
            var path = Project.Import(file);
            DisplayItem(path);
        }

        void DisplayAllItems()
        {
            var projPath = Project.ProjectsDirectory;
            var projects = Directory.GetDirectories(projPath);

            foreach (var project in projects)
            {
                if (Directory.Exists(project))
                    DisplayItem(project);
            }
        }

        void DisplayItem(string project)
        {
            try
            {
                LoadMenuItem item = Instantiate(itemPrefab, container);
                items.Add(item);
                item.SetPath(project);
            } catch { }
        }

        public void LoadProject(string project)
        {
            ItemsInteractable = false;
            data = Project.Load(project);
            VideoRenderer.SetState(new ConfirmPixelsState());
            DialogBox.Show(
                "Send loaded video buffer to lamps?",
                "Clicking \"OK\" will send loaded video to lamps, otherwise " +
                "Only lamp positions will be loaded, but lamps will still play " +
                "the video, they have at the moment.",
                "CANCEL", "OK",
                OnSendBufferCancel,
                OnSendBuffer);
        }

        void OnSendBufferCancel() => ItemsInteractable = true;

        void OnSendBuffer()
        {
            VideoRenderer.SetState(new DoneState());
            foreach (var lampData in data.lamps)
            {
                var video = VideoManager.instance.GetWithHash(lampData.video);
                var lamp = LampManager.instance.GetLampWithSerial(lampData.serial);
                lamp?.SetVideo(video);
            }
        }

        bool ItemsInteractable
        {
            set
            {
                foreach (var item in items)
                    item.button.interactable = value;
            }
        }
    }
}