using System;
using Newtonsoft.Json;
using UnityEngine;

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

        [JsonIgnore]
        public Color AsColor => Color.HSVToRGB(h, s, i);

        public static Itsh white => new Itsh(1.0f, DEFAULT_TEMPERATURE, 0.0f, 0.0f);

        public override string ToString() => $"[{i}, {t}, {s}, {h}]";
    }

    [Serializable]
    public struct Itshe
    {
        public const float DEFAULT_TEMPERATURE = 0.65884f;

        public float i;
        public float t;
        public float s;
        public float h;
        public float e;

        public Itshe(float i, float t, float s, float h, float e)
        {
            this.i = i;
            this.t = t;
            this.s = s;
            this.h = h;
            this.e = e;
        }

        public Itshe(Color color, float effect)
        {
            Color.RGBToHSV(color, out float h, out float s, out float i);
            this.i = i;
            this.s = s;
            this.h = h;

            t = DEFAULT_TEMPERATURE;
            e = effect;
        }

        public static implicit operator Itshe(Itsh itsh)
        {
            return new Itshe(itsh.i, itsh.t, itsh.s, itsh.h, 1.0f);
        }

        public static explicit operator Itsh(Itshe itshe)
        {
            return new Itsh(itshe.i, itshe.t, itshe.s, itshe.h);
        }

        [JsonIgnore]
        public Color AsColor => Color.HSVToRGB(h, s, i);

        public static Itshe white => new Itshe(1.0f, DEFAULT_TEMPERATURE, 0.0f, 0.0f, 1.0f);

        public override string ToString() => $"[{i}, {t}, {s}, {h}, {e}]";
    }
}