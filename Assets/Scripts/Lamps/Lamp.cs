﻿using System;
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
        public const double TIMEOUT = 5.0f;

        public string type;
        public string serial;
        public IPAddress address;
        public DmxSettings dmx;

        public bool connected => (TimeUtils.Epoch - lastMessage) < TIMEOUT;
        public double lastMessage;

        public virtual int pixelsCount { get; }

        public Video video;
        public Itsh itsh                    = Itsh.white;
        public VideoBuffer videoBuffer      = new VideoBuffer();

        internal virtual void Update(object data)
        {
            lastMessage = TimeUtils.Epoch;
        }

        internal virtual void UpdateDmxSettings(DmxSettings settings)
        {
            dmx = settings;
            lastMessage = TimeUtils.Epoch;
        }

        public virtual void SetItsh(Itsh itsh) { this.itsh = itsh; }
        public virtual void SetVideo(Video video) { this.video = video; }
        public abstract void PushFrame(Color32[] colors, long frame);

        public abstract LampItemView AddToWorkspace();
        public abstract LampItemView AddToWorkspace(Vector2 position);
        public abstract LampItemView AddToWorkspace(Vector2 position, float scale);
        public abstract LampItemView AddToWorkspace(Vector2 position, float scale, float rotation);
    }
}