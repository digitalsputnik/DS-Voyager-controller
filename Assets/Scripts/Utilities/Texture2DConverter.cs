using System;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

namespace VoyagerApp.Networking
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
            byte[] data = Convert.FromBase64String((string)reader.Value);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(data);
            return texture;
        }
    }
}