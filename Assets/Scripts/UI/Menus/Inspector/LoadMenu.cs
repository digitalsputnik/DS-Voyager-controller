using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using VoyagerApp.Effects;
using VoyagerApp.Lamps;
using VoyagerApp.Networking.Voyager;
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

        private void OnImportFile(string file)
        {
            string name = Path.GetFileNameWithoutExtension(file);

            if (items.Any(i => i.fileName == name))
            {
                DialogBox.Show(
                    "ALERT",
                    $"A project with name {name} already exists.",
                    new string[] { "OVERWRITE", "CANCEL" },
                    new Action[] {
                    () =>
                    {
                        items.FirstOrDefault(i => i.fileName == name)?.Delete(() =>
                        {
                            var path = Project.Import(file);
                            DisplayItem(path);
                        });
                    },
                        null
                    });
            }
            else
            {
                var path = Project.Import(file);
                DisplayItem(path);
            }
        }

        private void DisplayAllItems()
        {
            var projPath = Project.ProjectsDirectory;
            var projects = Directory.GetDirectories(projPath);

            foreach (var project in projects)
            {
                if (Directory.Exists(project))
                    DisplayItem(project);
            }
        }

        private void DisplayItem(string project)
        {
            try
            {
                LoadMenuItem item = Instantiate(itemPrefab, container);
                items.Add(item);
                item.SetPath(project);
            }
            catch
            {
                // ignored
            }
        }

        public void LoadProject(string project)
        {
            DialogBox.Show(
                "Send loaded video buffer to lamps?",
                "Clicking \"YES\" will send loaded video to lamps, otherwise " +
                "only lamp positions will be loaded, but lamps will still play " +
                "the video, they have at the moment.",
                new[] { "YES", "NO", "CANCEL" },
                new Action[] {
                    () =>
                    {
                        ItemsInteractable = false;
                        data = Project.Load(project);
                        OnSendBuffer();
                        VideoRenderer.SetState(new ConfirmPixelsState());
                    },
                    () =>
                    {
                        ItemsInteractable = false;
                        data = Project.Load(project, true);
                        OnSendBufferCancel();
                        VideoRenderer.SetState(new ConfirmPixelsState());
                    },
                    null
                }
            );
        }

        private void OnSendBufferCancel() => ItemsInteractable = true;

        private void OnSendBuffer()
        {
            VideoRenderer.SetState(new DoneState());
            foreach (var lampData in data.lamps)
            {
                var video = EffectManager.GetEffectWithId<Effects.Video>(lampData.effect);
                var image = EffectManager.GetEffectWithId<Effects.Image>(lampData.effect);
                var lamp = LampManager.instance.GetLampWithSerial(lampData.serial);
;
                var itshe = new Itshe
                {
                    i = lampData.itsh[0],
                    t = lampData.itsh[1],
                    s = lampData.itsh[2],
                    h = lampData.itsh[3],
                    e = lampData.itsh[4]
                };

                if (lamp == null) continue;

                if (video != null)
                {
                    lamp.effect = null;
                    lamp.SetEffect(video);
                    NetUtils.VoyagerClient.SendPacket(
                        lamp,
                        new SetPlayModePacket(PlaybackMode.Play, video.startTime, 0.0),
                        VoyagerClient.PORT_SETTINGS
                    );
                }
                
                if (image == null) continue;
                
                lamp.effect = null;
                lamp.SetEffect(image);
            }
        }

        private bool ItemsInteractable
        {
            set
            {
                foreach (var item in items)
                    item.button.interactable = value;
            }
        }
    }
}