using UnityEngine;
using VoyagerController.Effects;

namespace VoyagerController.Mapping
{
    public class EffectDisplaySettings : MonoBehaviour
    {
        private static readonly int _lift = Shader.PropertyToID("_Lift");
        private static readonly int _contrast = Shader.PropertyToID("_Contrast");
        private static readonly int _saturation = Shader.PropertyToID("_Saturation");
        private static readonly int _blurSize = Shader.PropertyToID("_BlurSize");
        
        private Material _material;

        private void Start()
        {
            _material = GetComponent<MeshRenderer>().sharedMaterial;
        }

        public void UpdateSettings(Effect effect)
        {
            ShaderUtils.ApplyEffectToMaterial(_material, effect);
        }
    }
}