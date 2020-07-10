using System.Linq;
using UnityEngine;
using VoyagerApp.Utilities;

namespace VoyagerApp.Videos
{
    public class RenderImageState : RenderState
    {
        public override RenderState Update()
        {
            foreach (var lamp in WorkspaceUtils.Lamps.Where(l => l.effect is Effects.Image))
            {
                var image = (Effects.Image)lamp.effect;
                var texture = image.image;
                
                var material = VideoRenderer.instance.renderMaterial;
                var render = new RenderTexture(texture.width, texture.height, 1, RenderTextureFormat.ARGB32);

                ShaderUtils.ApplyEffectToMaterial(material, image);

                var prevActive = RenderTexture.active;
                Graphics.Blit(texture, render, material);
                texture = TextureUtils.RenderTextureToTexture2D(render);
                RenderTexture.active = prevActive;

                var coords = VectorUtils.MapLampToVideoCoords(lamp, texture);
                var colors = TextureUtils.CoordsToColors(coords, texture);
                lamp.PushFrame(colors, 0);
            }
            return new ConfirmPixelsState();
        }

        public override void HandleEvent(VideoRenderEvent type) { }
    }
}