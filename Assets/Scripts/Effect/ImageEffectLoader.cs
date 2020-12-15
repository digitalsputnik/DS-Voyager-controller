using System;
using System.IO;
using UnityEngine;

namespace VoyagerApp.Effects
{
    public class ImageEffectLoader
    {
        public static Image LoadImageFromPath(string path)
        {
            var data = File.ReadAllBytes(path);
            var texture = new Texture2D(2, 2);
            texture.LoadImage(data);

            Image image = new Image
            {
                id = Guid.NewGuid().ToString(),
                name = Path.GetFileNameWithoutExtension(path),
                image = texture,
                thumbnail = texture
            };

            image.available.value = true;
            EffectManager.AddEffect(image);
            
            return image;
        }
    }
}