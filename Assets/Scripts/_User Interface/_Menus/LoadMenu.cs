using DigitalSputnik;
using DigitalSputnik.Voyager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using VoyagerController.Effects;
using VoyagerController.ProjectManagement;
using VoyagerController.Workspace;

namespace VoyagerController.UI
{
    public class LoadMenu : Menu
    {
        [SerializeField] private LoadMenuItem _itemPrefab = null;
        [SerializeField] private Transform _container = null;

        private readonly List<LoadMenuItem> _items = new List<LoadMenuItem>();
        private ProjectData _data;

        internal override void OnShow()
        {
            ClearOldItems();
            DisplayAllItems();
        }

        private void ClearOldItems()
        {
            new List<LoadMenuItem>(_items).ForEach(RemoveItem);
        }

        public void RemoveItem(LoadMenuItem item)
        {
            _items.Remove(item);
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

            if (_items.Any(i => i.fileName == name))
            {
                DialogBox.Show(
                    "ALERT",
                    $"A project with name {name} already exists.",
                    new string[] { "OVERWRITE", "CANCEL" },
                    new Action[] {
                    () =>
                    {
                        _items.FirstOrDefault(i => i.fileName == name)?.Delete(() => Project.Import(file, OnImportReady));
                    },
                        null
                    });
            }
            else
            {
                Project.Import(file, OnImportReady);
            }
        }

        private void OnImportReady(bool success, string path)
        {
            if (success)
                DisplayItem(path);

            Project.Load(Path.GetFileName(path));
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
                var item = Instantiate(_itemPrefab, _container);
                _items.Add(item);
                item.SetPath(project);
            }
            catch (Exception ex)
            {
                DebugConsole.LogError(ex);
            }
        }

        public void LoadProject(string project)
        {
            DialogBox.Show(
                "Are you sure?",
                "Loading a project will overwrite current settings on lamps. Would you like to load the project?",
                new[] { "YES", "CANCEL" },
                new Action[] {
                    () =>
                    {
                        ItemsInteractable = false;
                        _data = Project.Load(project);
                        ResendLampSettings(_data);
                        ItemsInteractable = true;
                    },
                    null
                }
            );
        }

        private static void ResendLampSettings(ProjectData data)
        {
            foreach (var lamp in WorkspaceManager.GetItems<VoyagerItem>().Select(i => i.LampHandle))
            {
                if (!lamp.Connected) continue;
                if (!data.LampMetadata.ContainsKey(lamp.Serial)) continue;

                if (lamp.DmxModeEnabled)
                {
                    lamp.ActivateDmxMode(lamp.DmxSettings);
                }
                else
                {
                    var meta = data.LampMetadata[lamp.Serial];
                    var effect = meta.Effect;
                    var buffer = meta.FrameBuffer;
                
                    LampEffectsWorker.ApplyEffectToLamp(lamp, effect);
                    Metadata.Get<LampData>(lamp.Serial).FrameBuffer = buffer;   
                }
            }
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
                foreach (var item in _items)
                    item.button.interactable = value;
            }
        }
    }
}