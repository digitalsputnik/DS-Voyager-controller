using UnityEngine;
using VoyagerController.Effects;

namespace VoyagerController
{
    public static class ShaderUtils
    {
        private static readonly int _lift = Shader.PropertyToID("_Lift");
        private static readonly int _contrast = Shader.PropertyToID("_Contrast");
        private static readonly int _saturation = Shader.PropertyToID("_Saturation");
        private static readonly int _blurSize = Shader.PropertyToID("_BlurSize");
        private static readonly int _standardDeviation = Shader.PropertyToID("_StandardDeviation");
        
        private static float GetEffectLift(Effect effect) => effect.Settings.Lift * 2.0f - 1.0f;
        private static float GetEffectContrast(Effect effect) => effect.Settings.Contrast * 4.0f - 1.0f;
        private static float GetEffectSaturation(Effect effect) => effect.Settings.Saturation * 2.0f;
        private static float GetEffectBlur(Effect effect) => effect.Settings.Blur * 0.4f;

        public static void ApplyEffectToMaterial(Material material, Effect effect)
        {
            material.SetFloat(_lift, GetEffectLift(effect));
            material.SetFloat(_contrast, GetEffectContrast(effect));
            material.SetFloat(_saturation, GetEffectSaturation(effect));

            if (effect.Settings.Blur > float.Epsilon)
            {
                material.SetFloat(_blurSize, GetEffectBlur(effect));
                material.SetFloat(_standardDeviation, 0.1f);
            }
            else
            {
                material.SetFloat(_standardDeviation, 0.0f);
            }
        }
    }
}