using UnityEngine;
using VoyagerController.Effects;
using VoyagerController.Rendering;

namespace VoyagerController.Mapping
{
    public class SpoutEffectDisplay : EffectDisplay
    {
        private MeshRenderer _meshRenderer;
        private readonly int _baseMap = Shader.PropertyToID("_MainTex");
        
        public override void Setup(Effect effect)
        {
            if (effect is SpoutEffect)
            {
                _meshRenderer = GetComponent<MeshRenderer>();
                _meshRenderer.sharedMaterial.SetTexture(_baseMap, SpoutRenderer.SpoutRenderTexture);
            }
        }

        public override void Clean()
        {
            _meshRenderer.sharedMaterial.SetTexture(_baseMap, null);
        }
    }
}