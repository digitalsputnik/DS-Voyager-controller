using Unity.Mathematics;
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

        public static Color32[] CoordsToColors(int2[] coords, Texture2D frame)
        {
            Color32[] colors = new Color32[coords.Length];
            for (int i = 0; i < coords.Length; i++)
            {
                if (coords[i].x == -1 && coords[i].y == -1)
                    colors[i] = Color.black;
                else
                    colors[i] = frame.GetPixel(coords[i].x, coords[i].y);
            }
            return colors;
        }
    }
}