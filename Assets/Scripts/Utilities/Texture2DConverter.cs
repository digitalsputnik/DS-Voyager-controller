using System;
using System.Text;
using Newtonsoft.Json;
using Unity.Mathematics;
using UnityEngine;

namespace VoyagerApp.Utilities
{
    class Texture2DConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Texture2D);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Texture2D texture = (Texture2D)value;
            byte[] data = texture.EncodeToJPG();
            writer.WriteValue(Convert.ToBase64String(data));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Texture2D texture = new Texture2D(120, 80);
            try
            {
                byte[] data = Convert.FromBase64String((string)reader.Value);
                texture.LoadImage(data);
                return texture;
            }
            catch
            {
                for (int y = 0; y < texture.height; y++)
                    for (int x = 0; x < texture.width; x++)
                        texture.SetPixel(x, y, Color.red);
                texture.Apply();
            }
            return texture;
        }
    }
}