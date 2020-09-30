using Unity.Mathematics;
using UnityEngine;

namespace VoyagerController
{
    public static class TextureExtensions
    {
        public static Texture2D ToTexture2D(this RenderTexture render)
        {
            var active = RenderTexture.active;
            RenderTexture.active = render;
            var texture = new Texture2D(render.width, render.height, TextureFormat.ARGB32, false);
            texture.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
            texture.Apply();
            RenderTexture.active = active;
            return texture;
        }
    }
}