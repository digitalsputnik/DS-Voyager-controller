using System.Linq;
using UnityEngine;
using VoyagerApp.Effects;
using VoyagerApp.Lamps;
using VoyagerApp.Utilities;

namespace VoyagerApp.Videos
{
    public class RenderImageState : RenderState
    {
        public override RenderState Update()
        {
            foreach (var effect in EffectManager.GetEffectsOfType<Image>())
            {
                var lamps = WorkspaceUtils.Lamps.Where(l => l.effect == effect);
                var enumerable = lamps as Lamp[] ?? lamps.ToArray();

                if (!enumerable.Any()) continue;
                
                var material = VideoRenderer.instance.renderMaterial;
                var render = new RenderTexture(effect.image.width, effect.image.height, 1, RenderTextureFormat.ARGB32);

                ShaderUtils.ApplyEffectToMaterial(material, effect);

                var prevActive = RenderTexture.active;
                    
                Graphics.Blit(effect.image, render, material);

                var texture = TextureUtils.RenderTextureToTexture2D(render);

                RenderTexture.active = prevActive;

                foreach (var lamp in enumerable)
                {
                    var coords = VectorUtils.MapLampToVideoCoords(lamp, texture);
                    var colors = TextureUtils.CoordsToColors(coords, texture);
                    lamp.PushFrame(colors, 0);
                }

                if (render != null) Object.Destroy(render);
                if (texture != null) Object.Destroy(texture);
            }
            
            return new ConfirmPixelsState();
        }

        public override void HandleEvent(VideoRenderEvent type) { }
    }
}