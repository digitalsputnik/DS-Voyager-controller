using System.Collections.Generic;
using System.Linq;
using DigitalSputnik.Voyager;
using UnityEngine;
using VoyagerController.Effects;
using VoyagerController.Workspace;

namespace VoyagerController.Rendering
{
    public class ImageRenderer : MonoBehaviour
    {
        private static ImageRenderer _instance;
        private void Awake() => _instance = this;

        [SerializeField] private Material _material;
        
        private void Update()
        {
            foreach (var voyager in LampsWithImageEffectNotRendered)
                RenderImage(voyager);
        }
        
        private static void RenderImage(VoyagerLamp voyager)
        {
            if (Metadata.Get<LampData>(voyager.Serial).Effect is ImageEffect effect)
            {
                var image = GetImageWithSettings(effect);
                var coords = TextureExtensions.MapLampToVideoCoords(voyager, image);
                var rgb =  TextureExtensions.CoordsToColors(coords.ToArray(), image).ToRgbArray();
                LampEffectsWorker.ApplyVideoFrameToVoyager(voyager, 0, rgb);
                Destroy(image);
            }
        }

        private static Texture2D GetImageWithSettings(ImageEffect effect)
        {
            var image = effect.ImageTexture;
            var render = new RenderTexture(image.width, image.height, 32);
            ShaderUtils.ApplyEffectToMaterial(_instance._material, effect);
            var prevActive = RenderTexture.active;
            Graphics.Blit(image, render, _instance._material);
            var texture = render.ToTexture2D();
            RenderTexture.active = prevActive;
            Destroy(render);
            return texture;
        }

        private static IEnumerable<VoyagerLamp> LampsWithImageEffectNotRendered => WorkspaceManager
            .GetItems<VoyagerItem>()
            .Where(v =>
            {
                var meta = Metadata.Get<LampData>(v.LampHandle.Serial);
                return meta.Effect is ImageEffect && !meta.ConfirmedFrames[0];
            })
            .Select(i => i.LampHandle);
    }
}