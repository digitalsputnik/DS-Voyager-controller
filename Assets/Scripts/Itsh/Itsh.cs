using System;
using Newtonsoft.Json;
using UnityEngine;
using VoyagerApp.Utilities;

namespace VoyagerApp
{
    [Serializable]
    public struct Itsh
    {
        public const float DEFAULT_TEMPERATURE = 0.65884f;

        public float i;
        public float t;
        public float s;
        public float h;

        public Itsh(float i, float t, float s, float h)
        {
            this.i = i;
            this.t = t;
            this.s = s;
            this.h = h;
        }

        public Itsh(Color color)
        {
            Color.RGBToHSV(color, out float h, out float s, out float i);
            this.i = i;
            this.t = DEFAULT_TEMPERATURE;
            this.s = s;
            this.h = h;
        }

        [JsonIgnore] public Color AsColor => ColorUtils.ItsheToRgb(new Itshe(i, t, s, h, 1.0f));

        public static Itsh white => new Itsh(1.0f, DEFAULT_TEMPERATURE, 0.0f, 0.0f);

        public override string ToString() => $"[{i}, {t}, {s}, {h}]";
    }
}