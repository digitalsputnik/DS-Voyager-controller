using System.Linq;
using UnityEngine;
using VoyagerApp.Utilities;

namespace VoyagerApp.Videos
{
    public class RenderImageState : RenderState
    {
        public override RenderState Update()
        {
            // TODO: Optimize
            
            foreach (var lamp in WorkspaceUtils.Lamps.Where(l => l.effect is Effects.Image))
            {
                var image = (Effects.Image)lamp.effect;

                var material = VideoRenderer.instance.renderMaterial;
                var render = new RenderTexture(image.image.width, image.image.height, 1, RenderTextureFormat.ARGB32);

                ShaderUtils.ApplyEffectToMaterial(material, image);

                var prevActive = RenderTexture.active;
                Graphics.Blit(image.image, render, material);

                var texture = TextureUtils.RenderTextureToTexture2D(render);

                RenderTexture.active = prevActive;

                var coords = VectorUtils.MapLampToVideoCoords(lamp, texture);
                var colors = TextureUtils.CoordsToColors(coords, texture);
                lamp.PushFrame(colors, 0);
                
                if (render != null) Object.Destroy(render);
                if (texture != null) Object.Destroy(texture);
            }
            return new ConfirmPixelsState();
        }

        public override void HandleEvent(VideoRenderEvent type) { }
    }
}