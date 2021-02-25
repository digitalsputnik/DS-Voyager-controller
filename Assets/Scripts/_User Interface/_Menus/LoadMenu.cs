using DigitalSputnik;
using DigitalSputnik.Voyager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;
using VoyagerController.Effects;
using VoyagerController.ProjectManagement;

namespace VoyagerController.UI
{
    public class LoadMenu : Menu
    {
        [SerializeField] LoadMenuItem itemPrefab = null;
        [SerializeField] Transform container = null;

        List<LoadMenuItem> items = new List<LoadMenuItem>();
        ProjectData data;

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
            //TODO
            //FileUtils.LoadProject(OnImportFile);
        }

        private void OnImportFile(string file)
        {
            /*string name = Path.GetFileNameWithoutExtension(file);

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
            }*/
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

                        Debug.Log(data.Version);
                        //OnSendBuffer();
                        //VideoRenderer.SetState(new ConfirmPixelsState());
                    },
                    () =>
                    {
                        ItemsInteractable = false;
                        data = Project.Load(project);

                        Debug.Log(data.Version);
                        //OnSendBufferCancel();
                        //VideoRenderer.SetState(new ConfirmPixelsState());
                    },
                    null
                }
            );
        }

        private void OnSendBufferCancel() => ItemsInteractable = true;

        private void OnSendBuffer()
        {
            /*VideoRenderer.SetState(new DoneState());
            foreach (var lampData in data.lamps)
            {
                var lamp = LampManager.instance.GetLampWithSerial(lampData.serial) as VoyagerLamp;
                if (lamp == null) continue;

                var video = EffectManager.GetEffectWithId<Effects.Video>(lampData.effect);
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

                var effect = EffectManager.GetEffectWithId(lampData.effect);
                if (effect == null) continue;

                lamp.effect = null;
                lamp.SetEffect(effect);

                StartCoroutine(LoadStream(effect, lampData, lamp));
            }*/
        }

        private static IEnumerator LoadStream(Effect effect, Lamp data, VoyagerLamp lamp)
        {
            /*if (!(effect is SyphonStream) && !(effect is SpoutStream))
                yield break;

            yield return new WaitForSeconds(0.2f);

            var frame = data.Buffer[0];
            var colors = ColorUtils.BytesToColors(frame);

            for (var i = 0; i < 5; i++)
            {
                var time = TimeUtils.Epoch + NetUtils.VoyagerClient.TimeOffset + 1.0f;
                lamp.PushStreamFrame(colors, time);
                yield return new WaitForSeconds(0.01f);
            }*/

            yield break;
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