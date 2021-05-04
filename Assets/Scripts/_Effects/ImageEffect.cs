using System.IO;
using UnityEngine;

namespace VoyagerController.Effects
{
    public class ImageEffect : Effect
    {
        public override string Name { get; }
        public Texture2D ImageTexture { get; }

        public ImageEffect(string path)
        {
            var file = File.ReadAllBytes(path);
            var texture = new Texture2D(4, 4);

            texture.LoadImage(file);
            texture.wrapMode = TextureWrapMode.Mirror;
            texture.Apply();

            Name = Path.GetFileNameWithoutExtension(path);
            ImageTexture = texture;
            Meta.Thumbnail = texture;
        }
    }
}