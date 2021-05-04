using UnityEngine;

namespace VoyagerController
{
    public static class Temperature
    {
        private static Gradient _temperatureGradient = null;

        private static Gradient GetTemperatureGradient()
        {
            if (_temperatureGradient != null) return _temperatureGradient;
            
            _temperatureGradient = new Gradient();

            var warmColor = new Color(0.98f, 0.678f, 0.459f);
            var midColor  = new Color(1.0f,  1.0f,   1.0f);
            var coldColor = new Color(0.78f, 0.855f, 1.0f);

            var warmKey = new GradientColorKey(warmColor, 0.0f);
            var midKey  = new GradientColorKey(midColor,  0.48f);
            var coldKey = new GradientColorKey(coldColor, 1.0f);

            var keys = new[] { warmKey, midKey, coldKey };

            _temperatureGradient.colorKeys = keys;

            return _temperatureGradient;
        }

        public static Color32 Apply(Color32 color, float temperature)
        {
            return color * GetTemperatureGradient().Evaluate(temperature);
        }

        public static Color32[] Apply(Color32[] colors, float temperature)
        {
            var cols = new Color32[colors.Length];
            for (var i = 0; i < colors.Length; i++)
                cols[i] = Apply(colors[i], temperature);
            return cols;
        }
    }
}