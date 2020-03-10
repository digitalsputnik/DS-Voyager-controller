using UnityEngine;
using VoyagerApp.Effects;

namespace VoyagerApp.Utilities
{
    public static class ShaderUtils
    {
        public static float GetEffectLift(Effect effect) => (effect.lift * 2.0f) - 1.0f;
        public static float GetEffectContrast(Effect effect) => (effect.contrast * 4.0f) - 1.0f;
        public static float GetEffectSaturation(Effect effect) => effect.saturation * 2.0f;
        public static float GetEffectBlur(Effect effect) => effect.blur * 0.4f;

        public static void ApplyEffectToMaterial(Material material, Effect effect)
        {
            material.SetFloat("_Lift", GetEffectLift(effect));
            material.SetFloat("_Contrast", GetEffectContrast(effect));
            material.SetFloat("_Saturation", GetEffectSaturation(effect));

            if (effect.blur > float.Epsilon)
            {
                material.SetFloat("_BlurSize", GetEffectBlur(effect));
                material.SetFloat("_StandardDeviation", 0.1f);
            }
            else
            {
                material.SetFloat("_StandardDeviation", 0.0f);
            }
        }
    }
}