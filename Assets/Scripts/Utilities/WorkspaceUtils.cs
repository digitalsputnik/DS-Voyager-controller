﻿using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using VoyagerApp.Lamps;
using VoyagerApp.Lamps.Voyager;
using VoyagerApp.Videos;
using VoyagerApp.Workspace;
using VoyagerApp.Workspace.Views;

namespace VoyagerApp.Utilities
{
    public static class WorkspaceUtils
    {
        public static List<ISelectableItem> SelectedItems
        {
            get => WorkspaceSelection.instance.Selected;
        }

        public static List<ISelectableItem> SelectableItems
        {
            get
            {
                List<ISelectableItem> items = new List<ISelectableItem>();
                WorkspaceManager.instance.Items
                    .ForEach(view =>
                    {
                        if (view is ISelectableItem item)
                            items.Add(item);
                    });
                return items;
            }
        }

        public static List<Lamp> SelectedLamps
        {
            get
            {
                List<Lamp> lamps = new List<Lamp>();
                WorkspaceSelection
                    .instance
                    .Selected
                    .ForEach(view =>
                    {
                        if (view is LampItemView lampView)
                            lamps.Add(lampView.lamp);
                    });
                return lamps;
            }
        }

        public static List<VoyagerLamp> SelectedVoyagerLamps
        {
            get
            {
                List<VoyagerLamp> lamps = new List<VoyagerLamp>();
                SelectedLamps.ForEach((lamp) =>
                {
                    if (lamp is VoyagerLamp vLamp)
                        lamps.Add(vLamp);
                });
                return lamps;
            }
        }

        public static List<Lamp> Lamps
        {
            get
            {
                List<Lamp> lamps = new List<Lamp>();
                WorkspaceManager.instance
                                .GetItemsOfType<LampItemView>().ToList()
                                .ForEach(view => lamps.Add(view.lamp));
                return lamps;
            }
        }

        public static List<VoyagerLamp> VoyagerLamps
        {
            get
            {
                List<VoyagerLamp> lamps = new List<VoyagerLamp>();
                WorkspaceManager.instance
                                .GetItemsOfType<VoyagerItemView>().ToList()
                                .ForEach(view => lamps.Add(view.lamp));
                return lamps;
            }
        }

        public static List<LampItemView> SelectedLampItems
        {
            get
            {
                List<LampItemView> lampItems = new List<LampItemView>();
                foreach (var selected in WorkspaceSelection.instance.Selected)
                {
                    if (selected is LampItemView view)
                        lampItems.Add(view);
                }
                return lampItems;
            }
        }

        public static List<VoyagerItemView> SelectedVoyagerLampItems
        {
            get
            {
                List<VoyagerItemView> lamps = new List<VoyagerItemView>();
                SelectedLampItems.ForEach((item) =>
                {
                    if (item is VoyagerItemView vItem)
                        lamps.Add(vItem);
                });
                return lamps;
            }
        }

        public static List<LampItemView> LampItems
        {
            get
            {
                return WorkspaceManager.instance
                                       .GetItemsOfType<LampItemView>()
                                       .ToList();
            }
        }

        public static List<VoyagerItemView> VoyagerItems
        {
            get
            {
                return WorkspaceManager.instance
                                       .GetItemsOfType<VoyagerItemView>()
                                       .ToList();
            }
        }

        public static void SelectAll()
        {
            foreach (var view in LampItems)
                WorkspaceSelection.instance.SelectItem(view);
        }

        public static void DeselectAll()
        {
            WorkspaceSelection.instance.Clear();
        }

        public static bool AtLastOneLampSelected
        {
            get => SelectedLamps.Count > 0;
        }

        public static bool AllLampsSelected
        {
            get => SelectedLamps.Count == Lamps.Count && Lamps.Count > 0;
        }

        public static bool SelectedLampsHaveSameVideo
        {
            get
            {
                if (SelectedLamps.Count == 0) return false;
                Video video = SelectedLamps[0].video;
                return SelectedLamps.All(l => l.video == video);
            }
        }

        public static void EnterToVideoMapping()
        {
            Video video = null;

            if (SelectedLampsHaveSameVideo)
            {
                video = SelectedLamps[0].video;
                if (video == null)
                {
                    video = VideoManager.instance.GetWithName("white");
                    foreach (var selected in SelectedLamps)
                        selected.SetVideo(video);
                }
            }

            new VideoMappingSettings(SelectedLamps, video).Save();
            Projects.Project.SaveWorkspace();
            SceneManager.LoadScene("Video Mapping");
        }

        public static void SelectLampsWithVideo(Video video)
        {
            foreach (var lampItem in LampItems)
            {
                if (lampItem.lamp.video == video)
                    WorkspaceSelection.instance.SelectItem(lampItem);
            }
        }

        public static Bounds SelectedLampsBounds()
        {
            var selected = SelectedItems;
            Bounds bounds = new Bounds(selected[0].SelectPositions[0], Vector3.zero);
            foreach (var lampItem in selected)
                foreach (var position in lampItem.SelectPositions)
                    bounds.Encapsulate(position);
            return bounds;
        }

        public static float2[] SelectedHorizontalAlignment()
        {
            Bounds bounds = SelectedLampsBounds();
            int count = SelectedLampItems.Count;
            List<float2> points = new List<float2>();

            //float step = bounds.size.x / (count - 1);
            //float start = bounds.center.x - bounds.size.x / 2.0f;
            float start = SelectedLampItems.Min(l => l.position.x);
            float max = SelectedLampItems.Max(l => l.position.x);
            float step = count > 1? (max - start) / (count - 1): 0;
            float y = bounds.center.y;

            for (int i = 0; i < count; i++)
                points.Add(new float2(start + step * i, y));

            return points.ToArray();
        }

        public static float2[] SelectedVerticalAlignment()
        {
            Bounds bounds = SelectedLampsBounds();
            int count = SelectedLampItems.Count;
            List<float2> points = new List<float2>();

            //float step = bounds.size.y / (count - 1);
            //float start = bounds.center.y + bounds.size.y / 2.0f;
            float start = SelectedLampItems.Min(l => l.position.y);
            float max = SelectedLampItems.Max(l => l.position.y);
            float step = count > 1 ? (max - start) / (count - 1) : 0;
            float x = bounds.center.x;

            for (int i = 0; i < count; i++)
                points.Add(new float2(x, start + step * i));

            return points.ToArray();
        }

        public static void AlignSelectedLampsHorizontally()
        {
            var rotations = new List<float> { 0.0f, 180.0f , 360.0f};
            AlignSelectedLampsToRotations(rotations);
        }

        public static void AlignSelectedLampsVertically()
        {
            var rotations = new List<float> { 90.0f, 270.0f };
            AlignSelectedLampsToRotations(rotations);
        }

        static void AlignSelectedLampsToRotations(List<float> rotations)
        {
            foreach (var lampItem in SelectedLampItems)
            {
                float2 position = lampItem.position;
                float scale = lampItem.scale;
                float rotation = rotations.OrderBy(x => math.abs(lampItem.rotation - x)).First();

                Lamp lamp = lampItem.lamp;
                WorkspaceManager.instance.RemoveItem(lampItem);
                var newItem = lamp.AddToWorkspace(position, scale, rotation);
                WorkspaceSelection.instance.SelectItem(newItem);
            }

            SelectionMove.RaiseMovedEvent();
        }

        public static void FlipSelectedLamps()
        {
            foreach (var lampItem in SelectedLampItems)
            {
                float2 position = lampItem.position;
                float rotation  = lampItem.rotation + 180.0f;
                float scale     = lampItem.scale;

                Lamp lamp = lampItem.lamp;

                WorkspaceManager.instance.RemoveItem(lampItem);
                var newItem = lamp.AddToWorkspace(position, scale, rotation);
                WorkspaceSelection.instance.SelectItem(newItem);
            }

            SelectionMove.RaiseMovedEvent();
        }

        public static void ScaleSelectedLampsBasedOnBiggest()
        {
            float scale = SelectedLampItems
                .OrderByDescending(l => l.scale)
                .FirstOrDefault().scale;

            foreach (var lampItem in SelectedLampItems)
            {
                float2 position = lampItem.position;
                float rotation = lampItem.rotation;

                Lamp lamp = lampItem.lamp;

                WorkspaceManager.instance.RemoveItem(lampItem);
                var newItem = lamp.AddToWorkspace(position, scale, rotation);
                WorkspaceSelection.instance.SelectItem(newItem);
            }

            SelectionMove.RaiseMovedEvent();
        }

        public static void DistributeSelectedLampsHorizontally()
        {
            var lamps = SelectedLampItems
                .OrderBy(l => l.position.x)
                .ToList();

            var positions = SelectedHorizontalAlignment();

            WorkspaceSelection.instance.Clear();

            for (int i = 0; i < lamps.Count; i++)
            {
                var lampItem = lamps[i];

                float2 position = positions[i];
                float scale = lampItem.scale;
                float rotation = lampItem.rotation;

                Lamp lamp = lampItem.lamp;

                WorkspaceManager.instance.RemoveItem(lampItem);
                var newItem = lamp.AddToWorkspace(position, scale, rotation);
                WorkspaceSelection.instance.SelectItem(newItem);
            }

            SelectionMove.RaiseMovedEvent();
        }

        public static void DistributeSelectedLampsVertically()
        {
            var lamps = SelectedLampItems
                .OrderBy(l => l.position.y)
                .ToList();

            var positions = SelectedVerticalAlignment();

            WorkspaceSelection.instance.Clear();

            for (int i = 0; i < lamps.Count; i++)
            {
                var lampItem = lamps[i];

                float2 position = positions[i];
                float scale = lampItem.scale;
                float rotation = lampItem.rotation;

                Lamp lamp = lampItem.lamp;

                WorkspaceManager.instance.RemoveItem(lampItem);
                var newItem = lamp.AddToWorkspace(position, scale, rotation);
                WorkspaceSelection.instance.SelectItem(newItem);
            }

            SelectionMove.RaiseMovedEvent();
        }

        public static List<LampItemView> SelectedLampItemsInOrder
        {
            get => SelectedLampItems.OrderBy(l => l.Order).ToList();
        }
    }
}