using UnityEngine;
using VoyagerController.Effects;
using VoyagerController.Rendering;

namespace VoyagerController.Mapping
{
    public class SyphonEffectDisplay : EffectDisplay
    {
        private MeshRenderer _meshRenderer;
        private readonly int _baseMap = Shader.PropertyToID("_MainTex");

        public override void Setup(Effect effect)
        {
            if (effect is SyphonEffect)
            {
                _meshRenderer = GetComponent<MeshRenderer>();
                _meshRenderer.sharedMaterial.SetTexture(_baseMap, SyphonRenderer.SyphonRenderTexture);
            }
        }

        public override void Clean()
        {
            _meshRenderer.sharedMaterial.SetTexture(_baseMap, null);
        }
    }
}