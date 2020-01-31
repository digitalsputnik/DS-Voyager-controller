using System;
using System.Net;
using UnityEngine;
using VoyagerApp.Effects;
using VoyagerApp.Utilities;
using VoyagerApp.Workspace.Views;

namespace VoyagerApp.Lamps
{
    [Serializable]
    public abstract class Lamp
    {
        public const double TIMEOUT = 15.0f;

        public event DataReceivedHandler OnDataReceived;

        public string type;
        public string serial;
        public IPAddress address;

        public virtual bool connected => (TimeUtils.Epoch - lastMessage) < TIMEOUT;
        public double lastMessage;
        public abstract int pixels { get; }
        public abstract string version { get; }
        public abstract bool updated { get; }
        public abstract double lastTimestamp { get; protected set; }

        public Itshe itshe = new Itshe();
        public Effect effect = null;
        public EffectMapping mapping = EffectMapping.Default;
        public VideoEffectBuffer buffer = new VideoEffectBuffer();

        internal void PushData(byte[] data)
        {
            OnDataReceived?.Invoke(data);
        }

        internal virtual void Update(object data)
        {
            lastMessage = TimeUtils.Epoch;
        }

        public virtual void SetItshe(Itshe itshe)
        {
            this.itshe = itshe;
            LampManager.instance.RaiseLampItsheChangedEvent(this);
        }

        public virtual void SetEffect(Effect effect)
        {
            this.effect = effect;
            LampManager.instance.RaiseLampEffectChangedEvent(this);
        }

        public virtual void PushFrame(Color32[] colors, long frame)
        {
            if (!buffer.FrameExists(frame))
                buffer.SetFrame(frame, ColorUtils.ColorsToBytes(colors));
        }

        public virtual void SetMapping(EffectMapping mapping)
        {
            this.mapping = mapping;
            LampManager.instance.RaiseLampMappingChangedEvent(this);
        }

        public abstract LampItemView AddToWorkspace();
        public abstract LampItemView AddToWorkspace(Vector2 position);
        public abstract LampItemView AddToWorkspace(Vector2 position, float scale);
        public abstract LampItemView AddToWorkspace(Vector2 position, float scale, float rotation);
    }

    public delegate void DataReceivedHandler(byte[] data);
}