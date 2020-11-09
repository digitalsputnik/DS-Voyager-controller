using UnityEngine;
using VoyagerController.Effects;

namespace VoyagerController.Mapping
{
    public class ImageEffectDisplay : EffectDisplay
    {
        private MeshRenderer _meshRenderer;
        private readonly int _baseMap = Shader.PropertyToID("_BaseMap");

        public override void Setup(Effect effect)
        {
            if (effect is ImageEffect image)
            {
                _meshRenderer = GetComponent<MeshRenderer>();
                _meshRenderer.sharedMaterial.SetTexture(_baseMap, image.ImageTexture);
            }
        }

        public override void Clean()
        {
            _meshRenderer.sharedMaterial.SetTexture(_baseMap, null);
        }
    }
}