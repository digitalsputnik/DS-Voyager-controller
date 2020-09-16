// -----------------------------------------------------------------
// Author: Taavet Maask	Date: 7/30/2019
// Copyright: © Digital Sputnik OÜ
// -----------------------------------------------------------------

using System;
using UnityEngine;

namespace VoyagerApp.Utilities
{
    public static class ColorUtils
    {
        static Gradient temperatureGradient = null;

        static Gradient GetTemperatureGradient()
        {
            if (temperatureGradient == null)
            {
                temperatureGradient = new Gradient();

                var warmColor = new Color(0.98f, 0.678f, 0.459f);
                var midColor  = new Color(1.0f,  1.0f,   1.0f);
                var coldColor = new Color(0.78f, 0.855f, 1.0f);

                var warmKey = new GradientColorKey(warmColor, 0.0f);
                var midKey  = new GradientColorKey(midColor,  0.48f);
                var coldKey = new GradientColorKey(coldColor, 1.0f);

                var keys = new GradientColorKey[] { warmKey, midKey, coldKey };

                temperatureGradient.colorKeys = keys;
            }

            return temperatureGradient;
        }

        public static Color32 ApplyTemperature(Color32 color, float temperature)
        {
            return color * GetTemperatureGradient().Evaluate(temperature);
        }

        public static Color32[] ApplyTemperature(Color32[] colors, float temperature)
        {
            var cols = new Color32[colors.Length];
            for (int i = 0; i < colors.Length; i++)
                cols[i] = ApplyTemperature(colors[i], temperature);
            return cols;
        }

        public static byte[] ColorsToBytes(Color32[] colors)
        {
            byte[] data = new byte[colors.Length * 3];

            for (int i = 0; i < colors.Length; i++)
            {
                Color32 color = colors[i];
                int index = i * 3;
                data[index + 0] = color.r;
                data[index + 1] = color.g;
                data[index + 2] = color.b;
            }

            return data;
        }

        public static Color32[] BytesToColors(byte[] bytes)
        {
            Color32[] colors = new Color32[bytes.Length / 3];

            for (int i = 0; i < colors.Length; i++)
            {
                Color32 color = new Color32();
                int index = i * 3;
                color.r = bytes[index + 0];
                color.g = bytes[index + 1];
                color.b = bytes[index + 2];
                color.a = 255;
                colors[i] = color;
            }

            return colors;
        }

        public static Color32[] MixColorsToItsh(Color32[] colors, Itsh itsh)
		{
			for (int i = 0; i < colors.Length; i++)
				colors[i] = colors[i] * itsh.AsColor;
			return colors;
		}

        public static Color32[] MixColorsToItshe(Color32[] colors, Itshe itshe, float gamma = 2.2f)
        {
            for (int i = 0; i < colors.Length; i++)
            {
                Color color = itshe.AsColor;
                Color videoColor = colors[i] * color;
                colors[i] = Color.Lerp(color, videoColor, Mathf.Pow(itshe.e, 1f/gamma));
            }
            return colors;
        }

        public static Color32[] ColorToColorArray(int size, Color32 color)
        {
            Color32[] colors = new Color32[size];
            for (int i = 0; i < size; i++)
                colors[i] = color;
            return colors;
        }

        public static Color32[] ApplyItensityCurve(this Color32[] colors, AnimationCurve curve)
        {
            for (int i = 0; i < colors.Length; i++)
            {
                Itshe itshe = new Itshe(colors[i], 0.0f);
                itshe.i = curve.Evaluate(itshe.i);
                colors[i] = itshe.AsColor;
            }

            return colors;
        }

        public static Color32[] LerpColorArray(Color32[] one, Color32[] two, float time)
        {
            Color32[] results = new Color32[one.Length];
            for (int i = 0; i < one.Length; i++)
                results[i] = Color32.Lerp(one[i], two[i], time);
            return results;
        }

        public static Color ToColor(this Itshe itshe) => ItsheToRgb(itshe);
        public static Color ItsheToRgb(Itshe itsh)
        {
            double r, g, b;
            double h = itsh.h;
            double i = itsh.i;
            double s = itsh.s;

            while (h < 0)
                h += 360;
            while (h >= 360)
                h -= 360;
            if (i <= 0)
                r = g = b = 0;
            else if (s <= 0)
                r = g = b = i;
            
            else
            {
                var hf = h / 60.0;
                var a = (int)Mathf.Floor((float)hf);
                var f = hf - a;
                var pv = i * (1 - s);
                var qv = i * (1 - s * f);
                var tv = i * (1 - s * (1 - f));
                
                switch (a)
                {
                    case 0:
                        r = i;
                        g = tv;
                        b = pv;
                        break;
                    case 1:
                        r = qv;
                        g = i;
                        b = pv;
                        break;
                    case 2:
                        r = pv;
                        g = i;
                        b = tv;
                        break;
                    case 3:
                        r = pv;
                        g = qv;
                        b = i;
                        break;
                    case 4:
                        r = tv;
                        g = pv;
                        b = i;
                        break;
                    case 5:
                        r = i;
                        g = pv;
                        b = qv;
                        break;
                    case 6:
                        r = i;
                        g = tv;
                        b = pv;
                        break;
                    case -1:
                        r = i;
                        g = pv;
                        b = qv;
                        break;
                    default:
                        r = g = b = i;
                        break;
                }
            }

            var color = Color.gray;
            color.r = Mathf.Clamp01((float)r);
            color.g = Mathf.Clamp01((float)g);
            color.b = Mathf.Clamp01((float)b);
            return color;
        }

        public static Itshe ToItshe(this Color color, float t = Itshe.DEFAULT_TEMPERATURE, float effect = 1.0f) =>
            RgbToItshe(color, t, effect);

        public static Itshe RgbToItshe(Color rgb, float t = Itshe.DEFAULT_TEMPERATURE, float effect = 1.0f)
        {
            double h = 0, s;

            var r = rgb.r * 255.0f;
            var g = rgb.g * 255.0f;
            var b = rgb.b * 255.0f;

            double min = Mathf.Min(Mathf.Min(r, g), b);
            double i = Mathf.Max(Mathf.Max(r, g), b);
            var delta = i - min;

            if (i == 0.0)
                s = 0;
            else
                s = delta / i;

            if (s == 0)
                h = 0.0;

            else
            {
                if (Math.Abs(r - i) < 0.001f)
                    h = (g - b) / delta;
                else if (Math.Abs(g - i) < 0.001f)
                    h = 2 + (b - r) / delta;
                else if (Math.Abs(b - i) < 0.001f)
                    h = 4 + (r - g) / delta;

                h *= 60;

                while (h < 0.0) h += 360;
            }

            return new Itshe((float)i, t, (float)s, (float)h, effect);
        }
    }

    public static class MathUtils
    {
        public static long Clamp(long value, long min, long max)
        {
            if (value > max) value = max;
            if (value < min) value = min;
            return value;
        }
    }
}