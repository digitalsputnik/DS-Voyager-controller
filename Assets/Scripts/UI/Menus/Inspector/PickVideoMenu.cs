using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VoyagerApp.Utilities;
using VoyagerApp.Videos;

namespace VoyagerApp.UI.Menus
{
    public class PickVideoMenu : Menu
    {
        [SerializeField] PickVideoItem itemPrefab   = null;
        [SerializeField] Transform container        = null;
        [SerializeField] GameObject loadingText     = null;

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
            foreach (var item in WorkspaceUtils.SelectedLamps)
                item.SetVideo(video);
        }

        internal override void OnShow()
        {
            VideoManager.instance.Videos.ForEach(AddVideoItem);
            VideoManager.instance.onVideoAdded += VideoAdded;
            VideoManager.instance.onVideoRemoved += VideoRemoved;
        }

        internal override void OnHide()
        {
            new List<PickVideoItem>(items).ForEach(RemoveVideoItem);
            VideoManager.instance.onVideoAdded -= VideoAdded;
            VideoManager.instance.onVideoRemoved -= VideoRemoved;
        }

        void VideoAdded(Video video) => AddVideoItem(video);

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
        }
    }
}