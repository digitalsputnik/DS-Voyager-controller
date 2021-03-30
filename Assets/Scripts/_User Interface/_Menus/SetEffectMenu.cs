using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DigitalSputnik;
using DigitalSputnik.Voyager;
using UnityEngine;
using VoyagerController.Effects;
using VoyagerController.Mapping;
using VoyagerController.Workspace;

namespace VoyagerController.UI
{
    public class SetEffectMenu : Menu
    {
        [SerializeField] private SetEffectItem _itemPrefab = null;
        [SerializeField] private Transform _container = null;
        [SerializeField] private bool _applyToSelected = true;
        [SerializeField] private Vector2Int _maxVideoSize;

        private static SetEffectMenu _instance;

        private readonly List<SetEffectItem> _items = new List<SetEffectItem>();

        internal override void OnShow()
        {
            _instance = this;
            EffectManager.OnEffectAdded += OnEffectEvent;
            EffectManager.OnEffectModified += OnEffectEvent;
            EffectManager.OnEffectRemoved += OnEffectEvent;
            WorkspaceSelection.SelectionChanged += OnWorkspaceSelectionChanged;
            UpdateEffectsList();
        }

        internal override void OnHide()
        {
            EffectManager.OnEffectAdded -= OnEffectEvent;
            EffectManager.OnEffectModified -= OnEffectEvent;
            EffectManager.OnEffectRemoved -= OnEffectEvent;
            WorkspaceSelection.SelectionChanged -= OnWorkspaceSelectionChanged;
        }

        private void OnEffectEvent(Effect effect) => UpdateEffectsList();

        private void UpdateEffectsList()
        {
            ClearItems();

            foreach (var effect in GetEffectsInOrder())
            {
                var item = Instantiate(_itemPrefab, _container);
                item.Setup(effect, () =>
                {
                    ValidateVideoResolution(effect, ApplyEffect, _ => ResizeAndApplyEffect(effect, item));
                });
                _items.Add(item);
            }
        }
        
        private void ClearItems()
        {
            foreach (var item in _items)
                Destroy(item.gameObject);
            _items.Clear();
        }
        
        private void OnWorkspaceSelectionChanged()
        {
            if (!WorkspaceSelection.GetSelected<VoyagerItem>().Any())
                GetComponentInParent<InspectorMenuContainer>().ShowMenu(null);
        }

        private void ApplyEffect(Effect effect)
        {
            if (_applyToSelected)
                ApplyEffectToSelectedLamps(effect);
            else
                ApplyEffectToAllLamps(effect);
        }

        private void ResizeAndApplyEffect(Effect effect, SetEffectItem item)
        {
            if (effect is VideoEffect video)
            {
                item.SetOverlay("Resizing...");
                
                VideoEffectLoader.ResizeVideo(video, _maxVideoSize.x, _maxVideoSize.y, done =>
                {
                    if (done == null)
                    {
                        DialogBox.Show(
                            "FAIL",
                            "Failed to change video resolution",
                            new [] { "OK" },
                            new Action[] { null });
                    }
                    else
                    {
                        ApplyEffect(effect);
                    }

                    item.SetOverlay(null);
                });
            }
        }

        private void ApplyEffectToSelectedLamps(Effect effect)
        {
            var voyagers = WorkspaceSelection.GetSelected<VoyagerItem>().ToList();

            StartCoroutine(ReselectLamps(voyagers));

            DialogBox.Show(
                "COPY LAMP POSITIONS?", 
                "Do you want to copy lamp lamp positions from workspace to FX mapping?",
                new [] { "YES", "NO", "CANCEL" },
                new Action[] {
                    () =>
                    {
                        UpdateLampsMappingPositionBasedOnSelection(voyagers);
                        EnterEffectMapping(effect, voyagers.Select(v => v.LampHandle));
                    },
                    () =>
                    {
                        EnterEffectMapping(effect, voyagers.Select(v => v.LampHandle));
                    },
                    null
                });
        }

        private void ValidateVideoResolution(Effect effect, Action<Effect> fine, Action<Effect> resize)
        {
            if (effect is VideoEffect video)
            {
                if (video.Video.Width > _maxVideoSize.x || video.Video.Height > _maxVideoSize.y)
                {
                    if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android)
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

        private static IEnumerator ReselectLamps(List<VoyagerItem> voyagers)
        {
            WorkspaceSelection.Clear();

            foreach (var lamp in voyagers)
            {
                yield return new WaitForFixedUpdate();
                WorkspaceSelection.SelectItem(lamp);
            }
        }

        private static void UpdateLampsMappingPositionBasedOnSelection(IEnumerable<VoyagerItem> voyagers)
        {
            var selection = WorkspaceManager.GetItems<SelectionControllerItem>().FirstOrDefault();

            if (selection == null) return;

            var trans = selection.GetComponentInChildren<MeshRenderer>().transform;
            var add = Vector3.one / 2.0f;
                
            foreach (var item in voyagers)
            {
                var p1 = item.GetPixelWorldPositions().First();
                var p2 = item.GetPixelWorldPositions().Last();

                p1 = trans.InverseTransformPoint(p1) + add;
                p2 = trans.InverseTransformPoint(p2) + add;

                var mapping = new EffectMapping
                {
                    X1 = p1.x, Y1 = p1.y,
                    X2 = p2.x, Y2 = p2.y
                };

                Metadata.Get<LampData>(item.LampHandle.Serial).EffectMapping = mapping;
            }
        }

        private void EnterEffectMapping(Effect effect, IEnumerable<VoyagerLamp> voyagers)
        {
            foreach (var voyager in voyagers)
                LampEffectsWorker.ApplyEffectToLamp(voyager, effect);
            GetComponentInParent<InspectorMenuContainer>().ShowMenu(null);
            EffectMapper.EnterEffectMapping(effect, _applyToSelected);
        }

        private void ApplyEffectToAllLamps(Effect effect)
        {
            foreach (var item in WorkspaceManager.GetItems<VoyagerItem>().Where(v => v.gameObject.activeSelf))
                LampEffectsWorker.ApplyEffectToLamp(item.LampHandle, effect);
            GetComponentInParent<InspectorMenuContainer>().ShowMenu(null);
            EffectMapper.EnterEffectMapping(effect, _applyToSelected);
        }

        private static IEnumerable<Effect> GetEffectsInOrder()
        {
            return EffectManager.GetEffects()
                .OrderByDescending(e => e.Meta.Timestamp)
                .ThenByDescending(e => e.Name == "white")
                //.ThenByDescending(e => e is SyphonEffect || e is SpoutEffect)
                .ThenByDescending(e => Metadata.Get<LampData>().Count(l => l.Effect == e))
                .ToList();
        }

        public void SetApplyToSelected(bool value) => _applyToSelected = value;

        public void AddEffect()
        {
            DialogBox.Show(
                "ADD EFFECT",
                "Pick which effect you want to add.",
                new [] { "IMAGE", "VIDEO", "CANCEL" },
                new Action[] { AddImageEffect, AddVideoEffect, null });
        }

        private void AddVideoEffect()
        {
            FileUtils.LoadVideoFromDevice(path =>
            {
                if (path != "" && path != "Null" && path != null)
                {
                    VideoEffectLoader.LoadVideoEffect(path, video =>
                    {
                        video.Meta.Timestamp = TimeUtils.Epoch;
                        UpdateEffectsList();
                    });
                }
            });
        }

        private void AddImageEffect()
        {
            FileUtils.LoadPictureFromDevice(path =>
            {
                if (path != "" && path != "Null" && path != null)
                {
                    var image = new ImageEffect(path) { Meta = { Timestamp = TimeUtils.Epoch }};
                    EffectManager.AddEffect(image);
                    UpdateEffectsList();
                }
            }, true);
        }
    }
}