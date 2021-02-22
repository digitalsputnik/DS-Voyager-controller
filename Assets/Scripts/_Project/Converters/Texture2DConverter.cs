using System;
using Newtonsoft.Json;
using UnityEngine;

namespace VoyagerController.ProjectManagement
{
    public class Texture2DConverter : JsonConverter<Texture2D>
    {
        public override void WriteJson(JsonWriter writer, Texture2D value, JsonSerializer serializer)
        {
            var data = value.EncodeToJPG();
            var json = Convert.ToBase64String(data);
            
            writer.WriteValue(json);
        }

        public override Texture2D ReadJson(JsonReader reader, Type objectType, Texture2D existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var data = reader.Value as string ?? "";
            var bytes = Convert.FromBase64String(data);
            var texture = new Texture2D(2, 2);
            
            texture.LoadImage(bytes);
            
            return texture;
        }
    }
}