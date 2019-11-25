﻿using UnityEngine;
using VoyagerApp.Lamps.Voyager;
using VoyagerApp.Networking;
using VoyagerApp.Networking.Voyager;
using VoyagerApp.UI;
using VoyagerApp.Utilities;
using VoyagerApp.Videos;

namespace VoyagerApp.Workspace.Views
{
    public class VoyagerItemView : LampItemView
    {
        [SerializeField] Transform pixels = null;
        [SerializeField] Vector2 pixelSize = Vector2.zero;
        [Space(3)]
        [SerializeField] Transform outline = null;
        [SerializeField] float outlineThickness = 0.0f;
        [SerializeField] AnimationCurve itensityCurve = null;
        [Space(3)]
        public new MeshRenderer renderer = null;
        public Material normMaterial = null;
        public Material dmxMaterial = null;

        Texture2D pixelsTexture;

        public new VoyagerLamp lamp;
        VoyagerClient client;

        bool playing = true;
        Color outlineColor;

        public long prevFrame;

        bool prevDmxEnabled;

        void LateUpdate()
        {
            if (prevDmxEnabled != lamp.dmxEnabled)
                SetupTextureAndMaterial();

            if (!lamp.dmxEnabled)
                RenderPixels();
        }

        public override void Setup(object data)
        {
            lamp = (VoyagerLamp)data;
            client = NetworkManager.instance.GetLampClient<VoyagerClient>();
            base.Setup(data);
            GlobalPlaymdoeChanged(ApplicationState.Playmode.value);
            ApplicationState.Playmode.onChanged += GlobalPlaymdoeChanged;
            outlineColor = outline.GetComponent<MeshRenderer>().material.color;
        }

        void OnDestroy()
        {
            ApplicationState.Playmode.onChanged -= GlobalPlaymdoeChanged;
        }

        void GlobalPlaymdoeChanged(GlobalPlaymode value)
        {
            switch (value)
            {
                case GlobalPlaymode.Play:
                    client.SendPacket(lamp, new SetPlayModePacket(PlaybackMode.Play), VoyagerClient.PORT_SETTINGS);
                    playing = true;
                    break;
                case GlobalPlaymode.Pause:
                    client.SendPacket(lamp, new SetPlayModePacket(PlaybackMode.Pause), VoyagerClient.PORT_SETTINGS);
                    playing = false;
                    prevFrame = TimeUtils.GetFrameOfVideo(lamp.video, -(TimeUtils.Epoch - ApplicationState.PlaymodePausedSince.value));
                    break;
                case GlobalPlaymode.Stop:
                    client.SendPacket(lamp, new SetPlayModePacket(PlaybackMode.Stop), VoyagerClient.PORT_SETTINGS);
                    prevFrame = 0;
                    playing = false;
                    break;
            }
        }

        protected override void Generate()
        {
            Vector2 size = new Vector2(lamp.length, 1) * pixelSize;
            pixels.localScale = size;

            Vector2 outlineSize = Vector2.one * outlineThickness;
            outline.localScale = size + outlineSize;

            nameText.transform.position = new Vector2(0, pixelSize.y * 0.75f);
            orderText.transform.position = new Vector2(-pixelSize.x * ((float)(lamp.length + 3) / 2), 0.0f);

            SetupTextureAndMaterial();
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

        void SetupTextureAndMaterial()
        {
            if (pixelsTexture == null)
            {
                Destroy(pixelsTexture);
                pixelsTexture = null;
            }

            if (!lamp.dmxEnabled)
            {
                pixelsTexture = new Texture2D(lamp.length, 1);
                pixelsTexture.filterMode = FilterMode.Point;
                pixelsTexture.Apply();

                renderer.material = normMaterial;
                renderer.material.SetTexture("_BaseMap", pixelsTexture);
            }
            else
            {
                renderer.material = dmxMaterial;
            }

            prevDmxEnabled = lamp.dmxEnabled;
        }

        void RenderPixels()
        {
            VideoBuffer buffer = lamp.buffer;

            if (buffer.ContainsVideo && lamp.video != null)
            {
                if (playing)
                {
                    long frame = TimeUtils.GetFrameOfVideo(lamp.video);
                    frame = buffer.GetClosestIndex(frame, 3);
                    if (buffer.FrameExists(frame))
                        DrawBufferFrame(buffer, frame);
                    prevFrame = frame;
                }
                else
                {
                    var frame = buffer.GetClosestIndex(prevFrame, 3);
                    if (buffer.FrameExists(frame))
                        DrawBufferFrame(buffer, frame);
                }
            }
            else
                DrawItshFrame();
        }

        void DrawBufferFrame(VideoBuffer buffer, long frame)
        {
            byte[] bytes = buffer.GetFrame(frame);
            Color32[] colors = ColorUtils.BytesToColors(bytes);
            colors = ColorUtils.MixColorsToItshe(colors, lamp.itshe);
            colors.ApplyItensityCurve(itensityCurve);
            PushToPixels(colors);
        }

        void DrawItshFrame()
        {
            Itshe defaultItsh = ApplicationSettings.AddedLampsDefaultColor;
            Color32[] colors = new Color32[lamp.length];
            for (int i = 0; i < lamp.length; i++)
                colors[i] = defaultItsh.AsColor;
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

        public override void Select()
        {
            base.Select();
            outline.transform.GetComponent<MeshRenderer>().material.color = selectedTextColor;
        }

        public override void Deselect()
        {
            base.Deselect();
            outline.transform.GetComponent<MeshRenderer>().material.color = outlineColor;
        }
    }
}
