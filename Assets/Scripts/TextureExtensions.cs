using System.Collections.Generic;
using DigitalSputnik.Voyager;
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
        
        public static Color32[] CoordsToColors(IReadOnlyList<Vector2Int> coords, Texture2D frame)
        {
            var colors = new Color32[coords.Count];
            for (var i = 0; i < coords.Count; i++)
            {
                if (coords[i].x == -1 && coords[i].y == -1)
                    colors[i] = Color.black;
                else
                    colors[i] = frame.GetPixel(coords[i].x, coords[i].y);
            }
            return colors;
        }

        public static IEnumerable<Vector2Int> MapLampToVideoCoords(VoyagerLamp voyager, Texture frame)
        {
            var mapping = Metadata.Get(voyager.Serial).EffectMapping;
            var coords = new Vector2Int[voyager.PixelCount];

            var p1 = new Vector2(mapping.X1, mapping.Y1);
            var p2 = new Vector2(mapping.X2, mapping.Y2);

            var delta = p2 - p1;
            var steps = delta / (coords.Length - 1);

            for (var i = 0; i < coords.Length; i++)
            {
                var x = p1.x + steps.x * i;
                var y = p1.y + steps.y * i;

                if (x > 1.0f || x < 0.0f || y > 1.0f || y < 0.0f)
                    coords[i] = new Vector2Int(-1, -1);
                else
                    coords[i] = new Vector2Int((int) (x * frame.width), (int) (y * frame.height));
            }

            return coords;
        }
    }
}