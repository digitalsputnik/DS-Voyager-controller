using System.Linq;
using UnityEngine;
using UnityEngine.Video;
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

        void Start()
        {
            player = GetComponent<VideoPlayer>();
            renderMesh = GetComponent<MeshRenderer>();

            ItemMove.onItemMoveEnded += ItemMoved;
        }

        void Update()
        {
            if (prevFrame == player.frame || !capture || player.frame == -1)
                return;

            Texture2D frame = TextureUtils.RenderTextureToTexture2D(render);
            PushPixelsToLamps(frame);
            Destroy(frame);

            if (TimeUtils.GetFrameOfVideo(video) == 0)
                SetFrame(TimeUtils.GetFrameOfVideo(video));
        }

        private void OnDestroy()
        {
            ItemMove.onItemMoveEnded -= ItemMoved;
        }

        #region Setting video
        public void SetVideo(Video video)
        {
            this.video = video;

            SetupRenderTexture();
            PrepereVideoPlayer();
            SetupMeshSize();
        }

        public void SetFrame(long frame)
        {
            player.frame = frame;
        }

        public void SetFps(float fps)
        {
            player.playbackSpeed = player.frameRate / fps;
            SetFrame(TimeUtils.GetFrameOfVideo(video));
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

        #region Handling events
        void ItemMoved(ItemMove move)
        {
            foreach (var item in WorkspaceUtils.LampItems)
            {
                var mapping = GetLampMapping(item);
                item.lamp.SetMapping(mapping);
                item.lamp.buffer.RecreateBuffer(video.frames);
            }
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
    }

    //public class VideoMapper : MonoBehaviour
    //{
    //    [SerializeField] Vector2 maxScale = Vector2.zero;

    //    VideoPlayer player; 
    //    Video video;
    //    MeshRenderer meshRenderer;
    //    RenderTexture render;

    //    long prevFrame = -1;

    //    int fps;
    //    bool stopRequested;
    //    bool running;

    //    double latestPauseStartTime;
    //    double pauseTime;

    //    void Start()
    //    {
    //        player = GetComponent<VideoPlayer>();
    //        meshRenderer = GetComponent<MeshRenderer>();
    //        meshRenderer.enabled = false;

    //        ItemMove.onItemMoveEnded += ItemMoved;

    //        PlayPauseStop.onPlay  +=  Play;
    //        PlayPauseStop.onPause +=  Pause;
    //        PlayPauseStop.onStop  +=  Stop;
    //    }

    //    void Update()
    //    {
    //        if (prevFrame == player.frame || !running) return;

    //        Texture2D frame = TextureUtils.RenderTextureToTexture2D(render);
    //        PushPixelsToLamps(frame);
    //        Destroy(frame);

    //        if (stopRequested)
    //        {
    //            player.Pause();
    //            stopRequested = false;
    //        }
    //    }

    //    public void Play()
    //    {
    //        if (latestPauseStartTime > 0.01)
    //            pauseTime += TimeUtils.Epoch - latestPauseStartTime;
    //        else if (latestPauseStartTime < -0.01)
    //        {
    //            float duration = video.frames / video.fps;
    //            double time = TimeUtils.Epoch - video.lastStartTime - pauseTime;
    //            pauseTime += time % duration;
    //        }

    //        if (video != null && !player.isPlaying)
    //            player.Play();
    //    }

    //    public void Pause()
    //    {
    //        latestPauseStartTime = TimeUtils.Epoch;

    //        if (video != null && !player.isPaused)
    //            player.Pause();
    //    }

    //    public void Stop()
    //    {
    //        latestPauseStartTime = -1;

    //        if (video != null)
    //        {
    //            player.frame = 0;
    //            stopRequested = true;
    //        }
    //    }

    //    public void SetVideo(Video video)
    //    {
    //        running = false;
    //        player.Stop();
    //        this.video = video;
    //        this.video.fps = fps;

    //        SetupRendererdTexture();

    //        player.url = video.path;
    //        player.started += PlayerStarted;

    //        player.Play();

    //        SetSize();

    //        meshRenderer.enabled = true;
    //    }

    //    public void SetFps(int fps)
    //    {
    //        if (video != null)
    //        {
    //            video.fps = fps;

    //            WorkspaceUtils.Lamps.ForEach(SendFps);

    //            if (player.isPlaying)
    //            {
    //                player.playbackSpeed = video.fps / player.frameRate;
    //                player.frame = GetFrameOfVideo();
    //            }
    //        }
    //        this.fps = fps;
    //    }

    //    long GetFrameOfVideo()
    //    {
    //        float duration = video.frames / video.fps;
    //        double time = TimeUtils.Epoch - video.lastStartTime - pauseTime;
    //        float videoTime = (float)time % duration;
    //        long frame = (long)(videoTime * video.fps);
    //        return MathUtils.Clamp(frame, 0, video.frames);
    //    }

    //    void OnDestroy()
    //    {
    //        ItemMove.onItemMoveEnded -= ItemMoved;

    //        PlayPauseStop.onPlay  -= Play;
    //        PlayPauseStop.onPause -= Pause;
    //        PlayPauseStop.onStop  -= Stop;
    //    }

    //    void ItemMoved(ItemMove move)
    //    {
    //        // TODO: Should be possible to send meta even if video does not exist.
    //        //       Position on video or itsh for examle.

    //        foreach (var item in WorkspaceUtils.LampItems)
    //        {
    //            var mapping = GetLampMapping(item);
    //            item.lamp.SetMapping(mapping);
    //            item.lamp.SetVideo(video);
    //        }
    //    }

    //    void PushPixelsToLamps(Texture2D frame)
    //    {
    //        foreach (var lamp in WorkspaceUtils.LampItems)
    //        {
    //            var coords = MapLampToVideoCoords(lamp, frame);
    //            var colors = CoordsToColors(coords, frame);
    //            lamp.PushColors(colors, player.frame);
    //        }
    //    }

    //    VideoPosition GetLampMapping(LampItemView lamp)
    //    {
    //        var allPixels = lamp.PixelWorldPositions();
    //        Vector2[] pixels = {
    //            allPixels.First(),
    //            allPixels.Last()
    //        };

    //        for (int i = 0; i < pixels.Length; i++)
    //        {
    //            Vector2 pos = pixels[i];
    //            Vector2 local = transform.InverseTransformPoint(pos);

    //            float x = local.x + 0.5f;
    //            float y = local.y + 0.5f;

    //            pixels[i] = new Vector2(x, y);
    //        }

    //        return new VideoPosition(pixels[0], pixels[1]);
    //    }

    //    Vector2Int[] MapLampToVideoCoords(LampItemView lamp, Texture2D frame)
    //    {
    //        Vector2[] pixelPositions = lamp.PixelWorldPositions();
    //        Vector2Int[] coords = new Vector2Int[pixelPositions.Length];

    //        for (int i = 0; i < pixelPositions.Length; i++)
    //        {
    //            Vector2 pos = pixelPositions[i];
    //            Vector2 local = transform.InverseTransformPoint(pos);

    //            float x = local.x + 0.5f;
    //            float y = local.y + 0.5f;

    //            if (x > 1.0f || x < 0.0f || y > 1.0f || y < 0.0f)
    //                coords[i] = new Vector2Int(-1, -1);
    //            else
    //                coords[i] = new Vector2Int((int)(x * frame.width),
    //                                           (int)(y * frame.height));
    //        }

    //        return coords;
    //    }

    //    Color32[] CoordsToColors(Vector2Int[] coords, Texture2D frame)
    //    {
    //        Color32[] colors = new Color32[coords.Length];
    //        for (int i = 0; i < coords.Length; i++)
    //        {
    //            if (coords[i].x == -1 && coords[i].y == -1)
    //                colors[i] = Color.black;
    //            else
    //                colors[i] = frame.GetPixel(coords[i].x, coords[i].y);
    //        }
    //        return colors;
    //    }

    //    void SendFps(Lamp lamp)
    //    {
    //        NetUtils.VoyagerClient.SendFpsAsMetadata(lamp);
    //    }

    //    void PlayerStarted(VideoPlayer source)
    //    {
    //        player.started -= PlayerStarted;

    //        // Claculate right frame
    //        double sinceStart = TimeUtils.Epoch - video.lastStartTime;
    //        long framesBasedStart = (long)(sinceStart * video.fps);
    //        long currentFrame = framesBasedStart % video.frames;
    //        player.frame = currentFrame;

    //        SetFps(fps);
    //        running = true;
    //    }

    //    void SetSize()
    //    {
    //        float maxScaleAspect = maxScale.x / maxScale.y;
    //        float videoAspect = (float)video.width / video.height;

    //        Vector2 s = maxScale;

    //        if (videoAspect > maxScaleAspect)
    //            s.y = maxScale.y / videoAspect * maxScaleAspect;
    //        else if (videoAspect < maxScaleAspect)
    //            s.x = maxScale.x / maxScaleAspect * videoAspect;

    //        transform.localScale = s;
    //    }

    //    void SetupRendererdTexture()
    //    {
    //        if (render != null)
    //            Destroy(render);

    //        render = new RenderTexture((int)video.width, (int)video.height, 32);
    //        render.Create();

    //        meshRenderer.material.mainTexture = render;
    //        player.targetTexture = render;
    //    }
    //}
}