using UnityEngine;
using VoyagerController.Effects;

namespace VoyagerController.Mapping
{
    public class ImageEffectDisplay : EffectDisplay
    {
        private MeshRenderer _meshRenderer;
        private readonly int _mainTex = Shader.PropertyToID("_MainTex");

        public override void Setup(Effect effect)
        {
            if (effect is ImageEffect image)
            {
                _meshRenderer = GetComponent<MeshRenderer>();
                _meshRenderer.sharedMaterial.SetTexture(_mainTex, image.ImageTexture);
            }
        }

        public override void Clean()
        {
            _meshRenderer.sharedMaterial.SetTexture(_mainTex, null);
        }
    }
}