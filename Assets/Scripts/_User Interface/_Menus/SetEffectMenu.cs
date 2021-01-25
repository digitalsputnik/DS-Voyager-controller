using System;
using System.Collections.Generic;
using System.Linq;
using Crosstales.FB;
using DigitalSputnik;
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

        public static SetEffectMenu _instance;

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
                    
                    EffectMapper.EnterEffectMapping(effect, _applyToSelected);
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
            foreach (var item in WorkspaceSelection.GetSelected<VoyagerItem>())
                LampEffectsWorker.ApplyEffectToLamp(item.LampHandle, effect);
            GetComponentInParent<InspectorMenuContainer>().ShowMenu(null);
        }

        private void ApplyEffectToAllLamps(Effect effect)
        {
            foreach (var item in WorkspaceManager.GetItems<VoyagerItem>())
                LampEffectsWorker.ApplyEffectToLamp(item.LampHandle, effect);
            GetComponentInParent<InspectorMenuContainer>().ShowMenu(null);
        }

        private static IEnumerable<Effect> GetEffectsInOrder()
        {
            return EffectManager.GetEffects()
                .OrderByDescending(e => e.Meta.Timestamp)
                .ThenByDescending(e => e.Name == "white.mp4")
                .ThenByDescending(e => Metadata.Get(l => l.Effect == e).Count())
                // .ThenByDescending(e => Metadata.Get(l => l.Effect is SyphonEffect || l.Effect is SpoutEffect))
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
            LoadVideoFromDevice(path =>
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
            LoadImageFromDevice(path =>
            {
                if (path != "" && path != "Null" && path != null)
                {
                    var image = new ImageEffect(path);
                    image.Meta.Timestamp = TimeUtils.Epoch;
                    EffectManager.AddEffect(image);
                    UpdateEffectsList();
                }
            });
        }

        private void Update()
        {
            /*if (AndroidVideoResizer.IsCompressing)
                Debug.Log(AndroidVideoResizer.Progress);*/
        }

        public static void LoadVideoFromDevice(Action<string> loaded)
        {
            if (Application.isMobilePlatform)
            {
                NativeGallery.GetVideoFromGallery(path =>
                {
                    if (string.IsNullOrEmpty(path))
                    {
                        loaded?.Invoke(null);
                        return;
                    }

                    if (Application.platform == RuntimePlatform.IPhonePlayer)
                    {
                        // TODO: Implement - the name should be asked from user.
                    }
                    else
                    {
                        loaded.Invoke(path);
                    }
                });
            }
            else
            {
                var documents = DocumentsPath;
                var extensions = new[] { new ExtensionFilter("Video", "mp4") };
                var path = FileBrowser.OpenSingleFile("Open Video", documents, extensions);
                loaded.Invoke(path == "" ? null : path);
            }
        }
        
        public static void LoadImageFromDevice(Action<string> loaded)
        {
            if (Application.isMobilePlatform)
            {
                if (Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    // TODO: Implement - the name should be asked from user.
                }
                else
                {
                    NativeGallery.GetImageFromGallery((string path) =>
                    {
                        loaded.Invoke(path == "" ? null : path);
                    }, "", "image/*");
                }
            }
            else
            {
                var documents = DocumentsPath;
                var extensions = new[]
                {
                    new ExtensionFilter("PNG Picture", "png"),
                    new ExtensionFilter("JPEG Picture", "jpeg"),
                    new ExtensionFilter("JPG Picture", "jpg")
                };
                var path = FileBrowser.OpenSingleFile("Open Picture", documents, extensions);
                loaded.Invoke(path == "" ? null : path);
            }
        }

        private static string DocumentsPath
        {
            get
            {
                if (Application.platform == RuntimePlatform.WindowsPlayer ||
                    Application.platform == RuntimePlatform.WindowsEditor)
                    return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                return Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            }
        }
    }
}