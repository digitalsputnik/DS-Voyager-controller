using System.Linq;
using UnityEngine;
using UnityEngine.Video;
using VoyagerApp.UI;
using VoyagerApp.Utilities;
using VoyagerApp.Workspace;
using VoyagerApp.Workspace.Views;

namespace VoyagerApp.Videos
{
    public class VideoMapper : MonoBehaviour
    {
        public Transform MeshTransform => renderMesh.transform;

        bool capture;
        int prevFrame;

        Video video;
        VideoPlayer player;
       
        RenderTexture render;
        MeshRenderer renderMesh;

        bool stopRequested;

        void Start()
        {
            player = GetComponent<VideoPlayer>();
            renderMesh = GetComponent<MeshRenderer>();

            ItemMove.onItemMoveEnded += ItemMoved;

            PlayPauseStop.onPlay  += OnPlayClicked;
            PlayPauseStop.onPause += OnPauseClicked;
            PlayPauseStop.onStop  += OnStopClicked;
        }

        void Update()
        {
            if (prevFrame == player.frame || !capture || player.frame == -1)
                return;

            Texture2D frame = TextureUtils.RenderTextureToTexture2D(render);
            PushPixelsToLamps(frame);
            Destroy(frame);

            if (TimeUtils.GetFrameOfVideo(video) == 0 && player.isPlaying)
                SetFrame(TimeUtils.GetFrameOfVideo(video));

            if (stopRequested)
            {
                player.Pause();
                stopRequested = false;
            }
        }

        private void OnDestroy()
        {
            ItemMove.onItemMoveEnded -= ItemMoved;

            PlayPauseStop.onPlay  -= OnPlayClicked;
            PlayPauseStop.onPause -= OnPauseClicked;
            PlayPauseStop.onStop  -= OnStopClicked;
        }

        #region Setting video
        public void SetVideo(Video video)
        {
            capture = false;

            if (render != null)
                ClearUp();

            this.video = video;

            if (video != null)
            {
                SetupRenderTexture();
                PrepereVideoPlayer();
                SetupMeshSize();
            }
        }

        public void SetFrame(long frame)
        {
            player.frame = frame;
        }

        public void SetFps(float fps)
        {
            player.playbackSpeed = 1.0f / player.frameRate * fps;
            SetFrame(TimeUtils.GetFrameOfVideo(video));
        }

        void ClearUp()
        {
            player.Stop();
            player.targetTexture = null;

            Destroy(render);

            video = null;
        }

        void SetupRenderTexture()
        {
            render = new RenderTexture((int)video.width, (int)video.height, 32);
            render.Create();

            renderMesh.material.mainTexture = render;
            player.targetTexture = render;
        }

        void PrepereVideoPlayer()
        {
            player.url = video.path;
            player.started += PlayerStarted;
            player.Play();
        }

        void PlayerStarted(VideoPlayer _)
        {
            player.started -= PlayerStarted;
            SetFps(video.fps);
            capture = true;
        }

        void SetupMeshSize()
        {
            Vector2 maxScale = CalculateMeshMaxScale();

            float maxScaleAspect = maxScale.x / maxScale.y;
            float videoAspect = (float)video.width / video.height;

            Vector2 s = maxScale;

            if (videoAspect > maxScaleAspect)
                s.y = maxScale.y / videoAspect * maxScaleAspect;
            else if (videoAspect < maxScaleAspect)
                s.x = maxScale.x / maxScaleAspect * videoAspect;

            transform.localScale = s;
        }

        Vector2 CalculateMeshMaxScale()
        {
            Vector2 screenWorldSize = VectorUtils.ScreenSizeWorldSpace;
            float width = screenWorldSize.x * 0.8f;
            float height = screenWorldSize.y - screenWorldSize.x * 0.2f;
            return new Vector2(width, height);
        }
        #endregion

        #region Mapping frame
        void PushPixelsToLamps(Texture2D frame)
        {
            foreach (var lamp in WorkspaceUtils.LampItems)
            {
                var coords = MapLampToVideoCoords(lamp, frame);
                var colors = CoordsToColors(coords, frame);
                lamp.PushColors(colors, player.frame);
            }
        }

        Vector2Int[] MapLampToVideoCoords(LampItemView lamp, Texture2D frame)
        {
            Vector2[] pixelPositions = lamp.PixelWorldPositions();
            Vector2Int[] coords = new Vector2Int[pixelPositions.Length];

            for (int i = 0; i < pixelPositions.Length; i++)
            {
                Vector2 pos = pixelPositions[i];
                Vector2 local = transform.InverseTransformPoint(pos);

                float x = local.x + 0.5f;
                float y = local.y + 0.5f;

                if (x > 1.0f || x < 0.0f || y > 1.0f || y < 0.0f)
                    coords[i] = new Vector2Int(-1, -1);
                else
                    coords[i] = new Vector2Int((int)(x * frame.width),
                                               (int)(y * frame.height));
            }

            return coords;
        }

        Color32[] CoordsToColors(Vector2Int[] coords, Texture2D frame)
        {
            Color32[] colors = new Color32[coords.Length];
            for (int i = 0; i < coords.Length; i++)
            {
                if (coords[i].x == -1 && coords[i].y == -1)
                    colors[i] = Color.black;
                else
                    colors[i] = frame.GetPixel(coords[i].x, coords[i].y);
            }
            return colors;
        }
        #endregion

        #region Handling position changed event
        void ItemMoved(ItemMove move)
        {
            var item = move.GetComponentInParent<WorkspaceItemView>();
            if (item == null) return;

            switch (item)
            {
                case LampItemView lampItem:
                    WorkspaceSelection.instance.Clear();
                    WorkspaceSelection.instance.SelectLamp(lampItem);
                    HandleLampMove(lampItem);
                    break;
                case SelectionControllerView selectionItem:
                    foreach (var lampItem in WorkspaceUtils.SelectedLampItems)
                        HandleLampMove(lampItem);
                    break;
            }
        }

        void HandleLampMove(LampItemView item)
        {
            var mapping = GetLampMapping(item);
            var lamp = item.lamp;

            if (video != null)
            {
                lamp.SetVideo(video);
                lamp.buffer.RecreateBuffer(video.frames);
            }

            lamp.SetMapping(mapping);
        }

        VideoPosition GetLampMapping(LampItemView lamp)
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

        #endregion

        #region Play / Pause / Stop
        private void OnPlayClicked(double pauseTime, bool fromStop)
        {
            if (pauseTime > 0.0)
			{
                video.lastStartTime += pauseTime;
                SetFrame(TimeUtils.GetFrameOfVideo(video));
			}

            if (fromStop)
            {
                video.lastStartTime = TimeUtils.Epoch;
                SetFrame(TimeUtils.GetFrameOfVideo(video));
            }

            if (video != null)
                player.Play();
        }

        private void OnPauseClicked()
        {
            if (video != null)
                player.Pause();
        }

        private void OnStopClicked()
        {
            if (video != null)
            {
                player.frame = 0;
                stopRequested = true;
            }
        }
        #endregion
    }
}