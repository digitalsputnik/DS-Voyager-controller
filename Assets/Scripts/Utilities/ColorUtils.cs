// -----------------------------------------------------------------
// Author: Taavet Maask	Date: 7/30/2019
// Copyright: © Digital Sputnik OÜ
// -----------------------------------------------------------------

using UnityEngine;
using VoyagerApp.Lamps;

namespace VoyagerApp.Utilities
{
    public static class ColorUtils
    {
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