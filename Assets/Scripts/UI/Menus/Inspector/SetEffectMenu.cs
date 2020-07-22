using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DigitalSputnik;
using UnityEngine;
using VoyagerApp.Effects;
using VoyagerApp.Lamps;
using VoyagerApp.Lamps.Voyager;
using VoyagerApp.Networking.Voyager;
using VoyagerApp.RefactoredEffects;
using VoyagerApp.UI.Overlays;
using VoyagerApp.Utilities;
using VoyagerApp.Workspace;
using VoyagerApp.Workspace.Views;
using Effect = VoyagerApp.Effects.Effect;

namespace VoyagerApp.UI.Menus
{
    public class SetEffectMenu : Menu
    {
        [SerializeField] SetEffectItem itemPrefab = null;
        [SerializeField] Transform container = null;
        [SerializeField] bool inWorkspace = true;
        [SerializeField] Vector2Int maxPreferedSized = new Vector2Int(1280, 720);

        List<SetEffectItem> items = new List<SetEffectItem>();

        public void AddEffectClicked()
        {
            DialogBox.Show(
                "Add Effect",
                "Pick which effect you want to add",
                new string[] { "IMAGE", "VIDEO", "CANCEL" },
                new Action[] { AddImageEffectClicked, AddVideoEffectClick, null }
            );
        }

        public void AddImageEffectClicked()
        {
            FileUtils.LoadPictureFromDevice(path => 
            {
                if (path != "" && path != "Null" && path != null)
                {
                    Image image = ImageEffectLoader.LoadImageFromPath(path);
                    image.timestamp = TimeUtils.Epoch;
                    OrderEffects();
                }
            });
        }

        public void AddVideoEffectClick()
        {
            FileUtils.LoadVideoFromDevice(path =>
            {
                if (path != "" && path != "Null" && path != null)
                {
                    var video = VideoEffectLoader.LoadNewVideoFromPath(path);
                    video.timestamp = TimeUtils.Epoch;
                    OrderEffects();
                }
            });
        }

        public void SelectEffect(Effect effect)
        {
            ValidateVideoResolution(effect, ApplyEffectToLamp, ResizeVideoEffect);
        }

        private void ValidateVideoResolution(Effect effect, Action<Effect> fine, Action<Video> resize)
        {
            if (effect is Video video)
            {
                if (video.width > maxPreferedSized.x || video.height > maxPreferedSized.y)
                {
                    if (Application.platform == RuntimePlatform.IPhonePlayer)
                    {
                        DialogBox.Show(
                            "WARNING",
                            "The video resolution is not supported and the application might suffer.",
                            new string[] { "CONTINUE", "RESIZE", "CANCEL" },
                            new Action[] { () => fine?.Invoke(effect), () => resize?.Invoke(video), null });
                    }
                    else
                    {
                        DialogBox.Show(
                            "WARNING",
                            "The video resolution is not supported and the application might suffer.",
                            new string[] { "CONTINUE", "CANCEL" },
                            new Action[] { () => fine?.Invoke(effect), null });
                    }
                }
                else fine?.Invoke(effect);
            }
            else fine?.Invoke(effect);
        }

        private void ApplyEffectToLamp(Effect effect)
        {
            GetComponentInParent<InspectorMenuContainer>().ShowMenu(null); // Move into initialization!

            if (inWorkspace)
                SelectEffectFromWorkspace(effect);
            else
                SelectEffectFromMapping(effect);
        }

        private void ResizeVideoEffect(Video effect)
        {
            var item = items.FirstOrDefault(i => i.effect == effect);
            
            if (item == null) return;
            
            App.VideoTools.LoadVideo(effect.path, video =>
            {  
                item.StartResizing();
                App.VideoTools.Resize(video, maxPreferedSized.x, maxPreferedSized.y, (success, error) =>
                {
                    if (success)
                    {
                        effect.width = (uint) video.Width;
                        effect.height = (uint) video.Height;
                        
                        item.StopResizing(effect);
                        // ApplyEffectToLamp(effect);
                    }
                    else
                    {
                        DialogBox.Show(
                            "FAIL",
                            "Failed to change video resolution",
                            new string[] { "OK", },
                            new Action[] { null });
                    }
                });
            });
        }

        public void RemoveEffect(Effect effect)
        {
            EffectManager.RemoveEffect(effect);
        }

        internal override void OnShow()
        {
            foreach (var effect in EffectManager.Effects)
                AddEffectItem(effect);

            OrderEffects();

            EffectManager.onEffectAdded += OnEffectAdded;
            EffectManager.onEffectRemoved += OnEffectRemoved;
            WorkspaceSelection.instance.onSelectionChanged += OnSelectionChanged;
        }

        internal override void OnHide()
        {
            foreach (var item in items.ToArray())
                RemoveEffectItem(item.effect);

            EffectManager.onEffectAdded -= OnEffectAdded;
            EffectManager.onEffectRemoved -= OnEffectRemoved;
            WorkspaceSelection.instance.onSelectionChanged -= OnSelectionChanged;
        }

        void OnSelectionChanged()
        {
            if (WorkspaceSelection.instance.Selected.Count == 0)
                GetComponentInParent<InspectorMenuContainer>().ShowMenu(null);
        }

        void OnEffectAdded(Effect effect)
        {
            AddEffectItem(effect);
            OrderEffects();
        }

        void OnEffectRemoved(Effect effect)
        {
            RemoveEffectItem(effect);
            OrderEffects();
        }

        void AddEffectItem(Effect effect)
        {
            var item = Instantiate(itemPrefab, container);
            item.SetEffect(effect);
            items.Add(item);
        }

        void RemoveEffectItem(Effect effect)
        {
            var item = items.FirstOrDefault(i => i.effect == effect);
            if (item == null) return;
            
            items.Remove(item);
            Destroy(item.gameObject);
        }

        void SelectEffectFromWorkspace(Effect effect)
        {
            DialogBox.Show(
                "COPY LAMP POSITIONS?",
                "Do you want to copy lamp positions from workspace to FX mapping?",
                new string[] { "YES", "NO", "CANCEL" },
                new Action[] {
                    () => SelectFromWorkspaceWithMapping(effect),
                    () => SelectFromWorkspaceWithoutMapping(effect),
                    null
                }
            );
        }

        void SelectFromWorkspaceWithMapping(Effect effect)
        {
            WorkspaceSelection.instance.ReselectItem();
            var selectionView = WorkspaceManager.instance
                .GetItemsOfType<SelectionControllerView>()[0];

            foreach (var view in WorkspaceUtils.SelectedLampItems)
                view.lamp.SetMapping(GetLampMapping(view, selectionView.render));

            SetEffectToLamps(WorkspaceUtils.SelectedLamps, effect);
            ApplicationState.Playmode.value = GlobalPlaymode.Play;
            WorkspaceUtils.EnterToVideoMapping();
            effect.timestamp = TimeUtils.Epoch;
        }

        void SelectFromWorkspaceWithoutMapping(Effect effect)
        {
            SetEffectToLamps(WorkspaceUtils.SelectedLamps, effect);
            ApplicationState.Playmode.value = GlobalPlaymode.Play;
            WorkspaceUtils.EnterToVideoMapping();
            effect.timestamp = TimeUtils.Epoch;
        }

        void SelectEffectFromMapping(Effect effect)
        {
            SetEffectToLamps(WorkspaceUtils.Lamps, effect);
            ApplicationState.Playmode.value = GlobalPlaymode.Play;
            effect.timestamp = TimeUtils.Epoch;
        }

        void OrderEffects()
        {
            var order = items
                .OrderByDescending(i => i.effect.timestamp)
                .ThenByDescending(i => i.effect.name == "white")
                .ThenByDescending(i => i.effect is SpoutStream || i.effect is SyphonStream)
                .ThenByDescending(i => LampManager.instance.LampsWithEffect(i.effect).Count)
                .ToList();

            for (int i = 0; i < order.Count; i++)
                order[i].transform.SetSiblingIndex(i);
        }

        static void SetEffectToLamps(List<Lamp> lamps, Effect effect)
        {
            foreach (var lamp in lamps)
            {
                if (lamp is VoyagerLamp vlamp && vlamp.dmxEnabled)
                {
                    var packet = new SetDmxModePacket(
                        false,
                        vlamp.dmxUniverse,
                        vlamp.dmxChannel,
                        vlamp.dmxDivision,
                        vlamp.dmxProtocol,
                        vlamp.dmxFormat
                    );

                    NetUtils.VoyagerClient.SendPacket(
                        lamp,
                        packet,
                        VoyagerClient.PORT_SETTINGS);
                }

                lamp.SetEffect(effect);

                if (effect is Video video)
                {
                    NetUtils.VoyagerClient.SendPacket(
                        lamp,
                        new SetPlayModePacket(PlaybackMode.Play, video.startTime, 0.0),
                        VoyagerClient.PORT_SETTINGS);
                }
            }
        }

        static EffectMapping GetLampMapping(LampItemView lamp, Transform transform)
        {
            var allPixels = lamp.PixelWorldPositions();
            Vector2[] pixels = {
                    allPixels.First(),
                    allPixels.Last()
            };

            for (int i = 0; i < pixels.Length; i++)
            {
                Vector2 pos = pixels[i];
                Vector2 local = transform.InverseTransformPoint(pos);

                float x = local.x + 0.5f;
                float y = local.y + 0.5f;

                pixels[i] = new Vector2(x, y);
            }

            return new EffectMapping(pixels[0], pixels[1]);
        }
    }
}