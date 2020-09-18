using System;
using Newtonsoft.Json;
using Unity.Mathematics;
using UnityEngine;
using VoyagerApp.Utilities;

namespace VoyagerApp
{
    [Serializable]
    public struct Itshe
    {
        public const float DEFAULT_TEMPERATURE = 0.482352941176471f;

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

        [JsonIgnore] public Color AsColor => Color.HSVToRGB(h, s, i); //this.ToColor();

        public static Itshe white => new Itshe(1.0f, DEFAULT_TEMPERATURE, 0.0f, 0.0f, 1.0f);

        public override string ToString() => $"[{i}, {t}, {s}, {h}, {e}]";

        public static bool operator ==(Itshe self, Itshe other)
        {
            return
                math.abs(self.i - other.i) < 0.001f &&
                math.abs(self.t - other.t) < 0.001f &&
                math.abs(self.s - other.s) < 0.001f &&
                math.abs(self.h - other.h) < 0.001f &&
                math.abs(self.e - other.e) < 0.001f;
        }

        public static bool operator !=(Itshe self, Itshe other)
        {
            return !(self == other);
        }

        public override bool Equals(object obj) 
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}