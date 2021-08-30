using UnityEngine;
using UnityEngine.UI;
using VoyagerController.Effects;

namespace VoyagerController.UI
{
    public class EffectMenuItem : MonoBehaviour
    {
        public Effect Effect { get; private set; }
        
        [SerializeField] private RawImage _image;
        [SerializeField] private AspectRatioFitter _aspect;

        public void SetEffect(Effect effect)
        {
            Effect = effect;
            _image.texture = effect.Meta.Thumbnail;
            _aspect.aspectRatio = (float) effect.Meta.Thumbnail.height / effect.Meta.Thumbnail.width;
        }
    }
}