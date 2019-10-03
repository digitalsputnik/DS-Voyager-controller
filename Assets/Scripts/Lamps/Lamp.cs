using System;
using System.Net;
using UnityEngine;
using VoyagerApp.Dmx;
using VoyagerApp.Utilities;
using VoyagerApp.Videos;
using VoyagerApp.Workspace.Views;

namespace VoyagerApp.Lamps
{
    [Serializable]
    public abstract class Lamp
    {
        public event DataReceivedHandler OnDataReceived;

        public const double TIMEOUT = 15.0f;

        public string type;
        public string serial;
        public IPAddress address;
        public DmxSettings dmx;

        public bool connected => (TimeUtils.Epoch - lastMessage) < TIMEOUT;
        public double lastMessage;
        public abstract int pixels { get; }
        public abstract string version { get; }
        public abstract bool updated { get; }
        public abstract double lastTimestamp { get; protected set; }

        public Itshe itshe;
        public Video video;
        public VideoPosition mapping;
        public VideoBuffer buffer;

        internal void PushData(byte[] data)
        {
            OnDataReceived?.Invoke(data);
        }

        internal virtual void Update(object data)
        {
            lastMessage = TimeUtils.Epoch;
        }

        internal virtual void UpdateDmxSettings(DmxSettings settings)
        {
            dmx = settings;
            lastMessage = TimeUtils.Epoch;
        }

        public virtual void SetItshe(Itshe itshe) => this.itshe = itshe;

        public virtual void SetVideo(Video video) => this.video = video;

        public virtual void PushFrame(Color32[] colors, long frame)
        {
            if (!buffer.FrameExists(frame))
                buffer.SetFrame(frame, ColorUtils.ColorsToBytes(colors));
        }

        public virtual void PushFrame(byte[] colors, long frame)
        {
            if (!buffer.FrameExists(frame))
                buffer.SetFrame(frame, colors);
        }

        public virtual void SetMapping(VideoPosition mapping) => this.mapping = mapping;

        public abstract LampItemView AddToWorkspace();
        public abstract LampItemView AddToWorkspace(Vector2 position);
        public abstract LampItemView AddToWorkspace(Vector2 position, float scale);
        public abstract LampItemView AddToWorkspace(Vector2 position, float scale, float rotation);
    }

    public delegate void DataReceivedHandler(byte[] data);
}