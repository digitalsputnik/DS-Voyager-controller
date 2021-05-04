using DigitalSputnik.Colors;
using UnityEngine;

namespace VoyagerController
{
    public static class MathUtils
    {
        public static float AngleTo(this Vector2 from, Vector2 to) => AngleFromTo(from, to);

        public static float AngleFromTo(Vector2 from, Vector2 to)
        {
            var direction = to - from;
            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            if (angle < 0f) angle += 360f;
            return angle;
        }

        public static Rgb[] ToRgbArray(this Color32[] colors)
        {
            var rgb = new Rgb[colors.Length];
            for (var i = 0; i < colors.Length; i++)
                rgb[i] = colors[i].ToRgb();
            return rgb;
        }
        
        public static Rgb ToRgb(this Color32 color) => new Rgb(color.r, color.g, color.b);

        public static Color32[] ToColorArray(this Rgb[] rgbs)
        {
            var colors = new Color32[rgbs.Length];
            for (var i = 0; i < rgbs.Length; i++)
                colors[i] = rgbs[i].ToColor();
            return colors;
        }
        
        public static Color32 ToColor(this Rgb rgb) => new Color32(rgb.RByte, rgb.GByte, rgb.BByte, 255);
        
        public static Color32 ToColor(this Itshe itshe) => ColorUtils.ItsheToRgb(itshe).ToColor();
    }
}