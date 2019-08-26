using UnityEngine;
using UnityEngine.Video;
using VoyagerApp.Lamps;
using VoyagerApp.UI;
using VoyagerApp.Utilities;
using VoyagerApp.Workspace;
using VoyagerApp.Workspace.Views;

namespace VoyagerApp.Videos
{
    public class VideoMapper : MonoBehaviour
    {
        [SerializeField] Vector2 maxScale = Vector2.zero;

        VideoPlayer player; 
        Video video;
        MeshRenderer meshRenderer;
        RenderTexture render;

        long prevFrame = -1;

        int fps;
        bool stopRequested;
        bool running;

        void Start()
        {
            player = GetComponent<VideoPlayer>();
            meshRenderer = GetComponent<MeshRenderer>();
            meshRenderer.enabled = false;

            ItemMove.onItemMoveEnded += ItemMoved;

            PlayPauseStop.onPlay  +=  Play;
            PlayPauseStop.onPause +=  Pause;
            PlayPauseStop.onStop  +=  Stop;
        }

        void Update()
        {
            if (prevFrame == player.frame || !running) return;

            Texture2D frame = TextureUtils.RenderTextureToTexture2D(render);
            PushPixelsToLamps(frame);
            Destroy(frame);

            if (stopRequested)
            {
                player.Pause();
                stopRequested = false;
            }
        }

        public void Play()
        {
            if (video != null && !player.isPlaying)
                player.Play();
        }

        public void Pause()
        {
            if (video != null && !player.isPaused)
                player.Pause();
        }

        public void Stop()
        {
            if (video != null)
            {
                player.frame = 0;
                stopRequested = true;
            }
        }

        public void SetVideo(Video video)
        {
            running = false;
            player.Stop();
            this.video = video;
            this.video.fps = fps;

            SetupRendererdTexture();

            player.url = video.path;
            player.started += PlayerStarted;

            SetFps(fps);

            player.Play();

            SetSize();

            meshRenderer.enabled = true;
        }

        public void SetFps(int fps)
        {
            if (video != null)
            {
                video.fps = fps;

                WorkspaceUtils.Lamps.ForEach(SendFps);

                if (player.isPlaying)
                    player.playbackSpeed = video.fps / player.frameRate;
            }
            this.fps = fps;
        }

        void OnDestroy()
        {
            ItemMove.onItemMoveEnded -= ItemMoved;

            PlayPauseStop.onPlay  -= Play;
            PlayPauseStop.onPause -= Pause;
            PlayPauseStop.onStop  -= Stop;
        }

        void ItemMoved(ItemMove item)
        {
            // TODO: Should be possible to send meta even if video does not exist.
            //       Position on video or itsh for examle.

            if (video != null)
                WorkspaceUtils.LampItems.ForEach(_ => _.lamp.SetVideo(video));
        }

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

        void SendFps(Lamp lamp)
        {
            NetUtils.VoyagerClient.SendFpsAsMetadata(lamp);
        }

        void PlayerStarted(VideoPlayer source)
        {
            player.started -= PlayerStarted;
            player.playbackSpeed = video.fps / player.frameRate;
            video.lastStartTime = TimeUtils.Epoch;

            WorkspaceUtils.Lamps.ForEach(l => l.SetVideo(video));
            running = true;
        }

        void SetSize()
        {
            float maxScaleAspect = maxScale.x / maxScale.y;
            float videoAspect = (float)video.width / video.height;

            Vector2 s = maxScale;

            if (videoAspect > maxScaleAspect)
                s.y = maxScale.y / videoAspect * maxScaleAspect;
            else if (videoAspect < maxScaleAspect)
                s.x = maxScale.x / maxScaleAspect * videoAspect;

            transform.localScale = s;
        }

        void SetupRendererdTexture()
        {
            if (render != null)
                Destroy(render);

            render = new RenderTexture((int)video.width, (int)video.height, 32);
            render.Create();

            meshRenderer.material.mainTexture = render;
            player.targetTexture = render;
        }
    }
}