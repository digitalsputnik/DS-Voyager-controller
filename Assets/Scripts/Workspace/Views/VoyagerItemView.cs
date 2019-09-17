using UnityEngine;
using VoyagerApp.Lamps.Voyager;
using VoyagerApp.Networking;
using VoyagerApp.UI;
using VoyagerApp.Utilities;
using VoyagerApp.Videos;

namespace VoyagerApp.Workspace.Views
{
    public class VoyagerItemView : LampItemView
    {
        [SerializeField] Transform pixels           = null;
        [SerializeField] Vector2 pixelSize          = Vector2.zero;
        [Space(3)]
        [SerializeField] Transform outline          = null;
        [SerializeField] float outlineThickness     = 0.0f;
        [Space(3)]
        public new MeshRenderer renderer            = null;

        Texture2D pixelsTexture;

        public new VoyagerLamp lamp;
        VoyagerClient client;

        bool playing = true;

        void LateUpdate()
        {
            RenderPixels();
        }

        public override void Setup(object data)
        {
            lamp = (VoyagerLamp)data;
            client = NetworkManager.instance.GetLampClient<VoyagerClient>();

            PlayPauseStop.onPlay  += OnPlay;
            PlayPauseStop.onPause += OnPause;
            PlayPauseStop.onStop  += OnStop;

            base.Setup(data);
        }

        void OnDestroy()
        {
            PlayPauseStop.onPlay  -= OnPlay;
            PlayPauseStop.onPause -= OnPause;
            PlayPauseStop.onStop  -= OnStop;
        }

        void OnPlay(double startTime, bool fromStop)
        {
            client.SendPlaymode(lamp, VoyagerPlaybackMode.Play);
            playing = true;
        }

        void OnPause()
        {
            client.SendPlaymode(lamp, VoyagerPlaybackMode.Pause);
            playing = false;
        }

        void OnStop()
        {
            client.SendPlaymode(lamp, VoyagerPlaybackMode.Stop);
            var buffer = lamp.buffer;
            long frame = buffer.GetClosestIndex(0, 3);
            if (buffer.FrameExists(frame))
                DrawBufferFrame(buffer, frame);
            playing = false;
        }

        protected override void Generate()
        {
            Vector2 size = new Vector2(lamp.length, 1) * pixelSize;
            pixels.localScale = size;

            Vector2 outlineSize = Vector2.one * outlineThickness;
            outline.localScale = size + outlineSize;

            nameText.transform.position = new Vector2(0, pixelSize.y * 0.75f);

            pixelsTexture = new Texture2D(lamp.length, 1);
            pixelsTexture.filterMode = FilterMode.Point;
            pixelsTexture.Apply();

            renderer.material.mainTexture = pixelsTexture;
        }

        public override Vector2[] PixelWorldPositions()
        {
            Vector2[] positions = new Vector2[lamp.length];
            float distance = 1.0f / lamp.length;
            float offset = distance / 2.0f;

            for (int i = 0; i < lamp.length; i++)
            {
                float x = i * distance - 0.5f + offset;
                Vector2 local = new Vector2(x, 0.0f);
                positions[i] = pixels.TransformPoint(local);
            }

            return positions;
        }

        public override void PushColors(Color32[] colors, long frame)
        {
            lamp.PushFrame(colors, frame);
        }

        void RenderPixels()
        {
            VideoBuffer buffer = lamp.buffer;

            if (buffer.ContainsVideo)
            {
                if (playing)
                {
                    long frame = TimeUtils.GetFrameOfVideo(lamp.video);
                    frame = buffer.GetClosestIndex(frame, 3);
                    if (buffer.FrameExists(frame))
                        DrawBufferFrame(buffer, frame);
                }
            }
            else DrawItshFrame();
        }

        void DrawBufferFrame(VideoBuffer buffer, long frame)
        {
            byte[] bytes = buffer.GetFrame(frame);
            Color32[] colors = ColorUtils.BytesToColors(bytes);
            colors = ColorUtils.MixColorsToItshe(colors, lamp.itshe);
            PushToPixels(colors);
        }

        void DrawItshFrame()
        {
            Color32[] colors = new Color32[lamp.length];
            for (int i = 0; i < lamp.length; i++)
                colors[i] = lamp.itshe.AsColor;
            PushToPixels(colors);
        }

        void PushToPixels(Color32[] colors)
        {
            if (colors.Length == lamp.pixels)
            {
                pixelsTexture.SetPixels32(colors);
                pixelsTexture.Apply();
            }
        }
    }
}
