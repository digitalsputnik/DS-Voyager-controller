using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VoyagerApp.Networking.Voyager;
using VoyagerApp.UI.Overlays;
using VoyagerApp.Utilities;
using VoyagerApp.Videos;
using VoyagerApp.Workspace;
using VoyagerApp.Workspace.Views;

namespace VoyagerApp.UI.Menus
{
    public class PickVideoMenu : Menu
    {
        [SerializeField] PickVideoItem itemPrefab = null;
        [SerializeField] Transform container = null;
        [SerializeField] GameObject loadingText = null;
        [Space(5)]
        [SerializeField] bool inVideoMapping = false;
        [SerializeField] VideoMappingController mapping = null;

        List<PickVideoItem> items = new List<PickVideoItem>();

        public void AddNewVideo()
        {
            FileUtils.LoadVideoFromDevice(path =>
            {
                if (path != "" && path != "Null" && path != null)
                {
                    VideoLoadingStarted();
                    VideoManager.instance.LoadVideo(path, VideoLoadingCompleted);
                }
            });
        }

        public void PickVideo(Video video)
        {
            GetComponentInParent<InspectorMenuContainer>().ShowMenu(null);

            if (!inVideoMapping)
            {
                DialogBox.Show(
                    "COPY LAMP POSITIONS?",
                    "Click \"OK\" to copy lamp positions from workspace to FX mapping.",
                    "CANCEL", "OK",
                    () =>
                    {
                        foreach (var lamp in WorkspaceUtils.SelectedVoyagerLamps)
                        {
                            var packet = new SetDmxModePacket(
                                false,
                                lamp.dmxUniverse,
                                lamp.dmxChannel,
                                lamp.dmxDivision,
                                lamp.dmxProtocol,
                                lamp.dmxFormat
                            );
                            NetUtils.VoyagerClient.SendPacket(lamp, packet, VoyagerClient.PORT_SETTINGS);
                        }

                        foreach (var item in WorkspaceUtils.SelectedLamps)
                            item.SetVideo(video);
                        WorkspaceUtils.EnterToVideoMapping();
                    },
                    () =>
                    {
                        foreach (var lamp in WorkspaceUtils.SelectedVoyagerLamps)
                        {
                            var packet = new SetDmxModePacket(
                                false,
                                lamp.dmxUniverse,
                                lamp.dmxChannel,
                                lamp.dmxDivision,
                                lamp.dmxProtocol,
                                lamp.dmxFormat
                            );
                            NetUtils.VoyagerClient.SendPacket(lamp, packet, VoyagerClient.PORT_SETTINGS);
                        }

                        var selectionView = WorkspaceManager.instance.GetItemsOfType<SelectionControllerView>()[0];
                        foreach (var view in WorkspaceUtils.SelectedLampItems)
                        {
                            view.lamp.SetMapping(GetLampMapping(view, selectionView.render));
                            view.lamp.SetVideo(video);
                        }
                        WorkspaceUtils.EnterToVideoMapping();
                    });
            }
            else
            {
                foreach (var lamp in WorkspaceUtils.Lamps)
                    lamp.SetVideo(video);

                var lamps = WorkspaceUtils.Lamps;
                var selection = WorkspaceUtils.SelectedLamps;

                WorkspaceManager.instance.Clear();

                mapping.SetVideo(video);
                mapping.PositionLamps(lamps);

                foreach (var view in WorkspaceUtils.LampItems)
                {
                    if (selection.Contains(view.lamp))
                        WorkspaceSelection.instance.SelectItem(view);
                }
            }
        }

        static VideoPosition GetLampMapping(LampItemView lamp, Transform transform)
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

            return new VideoPosition(pixels[0], pixels[1]);
        }

        internal override void OnShow()
        {
            VideoManager.instance.Videos.ForEach(AddVideoItem);
            VideoManager.instance.onVideoAdded += VideoAdded;
            VideoManager.instance.onVideoRemoved += VideoRemoved;
            SortVideoItems();
        }

        internal override void OnHide()
        {
            new List<PickVideoItem>(items).ForEach(RemoveVideoItem);
            VideoManager.instance.onVideoAdded -= VideoAdded;
            VideoManager.instance.onVideoRemoved -= VideoRemoved;
        }

        void VideoAdded(Video video)
        {
            AddVideoItem(video);
            SortVideoItems();
        }

        void VideoRemoved(Video video)
        {
            PickVideoItem item = items.FirstOrDefault(_ => _.video == video);
            if (item != null) RemoveVideoItem(item);
        }

        void AddVideoItem(Video video)
        {
            PickVideoItem item = Instantiate(itemPrefab, container);
            item.SetVideo(video);
            items.Add(item);

            if (Application.platform == RuntimePlatform.Android)
            {
                if (video.width > 640 || video.height > 360)
                {
                    DialogBox.Show(
                        "WARNING",
                        $"Video {video.name} is bigger than 640x360 and might not " +
                        $"play correctly",
                        "DELETE", "OK",
                        item.Remove, null);
                }
            }
        }

        void RemoveVideoItem(PickVideoItem item)
        {
            items.Remove(item);
            Destroy(item.gameObject);
        }

        void VideoLoadingStarted()
        {
            loadingText.SetActive(true);
        }

        void VideoLoadingCompleted(Video video)
        {
            loadingText.SetActive(false);
            SortVideoItems();
        }

        void SortVideoItems()
        {
            var sorted = items
                .OrderByDescending(i => i.video.name == "white")
                .ThenByDescending(i => WorkspaceUtils.Lamps.Count(l => l.video == i.video))
                .ThenByDescending(i => i.name)
                .ToList();

            for (int i = 0; i < sorted.Count; i++)
                sorted[i].transform.SetSiblingIndex(i);
        }
    }
}