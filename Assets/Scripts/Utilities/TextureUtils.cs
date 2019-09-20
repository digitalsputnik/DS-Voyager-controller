using UnityEngine;

namespace VoyagerApp.Utilities
{
    public static class TextureUtils
    {
        public static Texture2D RenderTextureToTexture2D(RenderTexture render)
        {
            RenderTexture active = RenderTexture.active;
            RenderTexture.active = render;
            Texture2D texture = new Texture2D(render.width, render.height, TextureFormat.ARGB32, false);
            texture.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
            texture.Apply();
            RenderTexture.active = active;
            return texture;
        }
    }
}