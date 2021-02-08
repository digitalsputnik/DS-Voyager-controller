using System;
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
                    if (_applyToSelected)
                        ApplyEffectToSelectedLamps(effect);
                    else
                        ApplyEffectToAllLamps(effect);
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

        private void ApplyEffectToSelectedLamps(Effect effect)
        {
            var voyagers = WorkspaceSelection.GetSelected<VoyagerItem>().ToList();
            
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

                Metadata.Get(item.LampHandle.Serial).EffectMapping = mapping;
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
            foreach (var item in WorkspaceManager.GetItems<VoyagerItem>())
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
                .ThenByDescending(e => Metadata.Get(l => l.Effect == e).Count())
                .ToList();
        }

        public void SetApplyToSelected(bool value) => _applyToSelected = value;

        public void AddEffect()
        {
            DialogBox.Show(
                "WHAT EFFECT?",
                "Pick which effect you would like to add.",
                new string[] { "VIDEO", "IMAGE" },
                new Action[] { AddVideoEffect, AddImageEffect });
        }

        public void AddVideoEffect()
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

        public void AddImageEffect()
        {
            FileUtils.LoadPictureFromDevice(path =>
            {
                if (path != "" && path != "Null" && path != null)
                {
                    var image = new ImageEffect(path);
                    image.Meta.Timestamp = TimeUtils.Epoch;
                    EffectManager.AddEffect(image);
                    UpdateEffectsList();
                }
            }, true);
        }
    }
}