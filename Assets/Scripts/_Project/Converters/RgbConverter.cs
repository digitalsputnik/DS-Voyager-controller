using System;
using DigitalSputnik.Colors;
using Newtonsoft.Json;

namespace VoyagerController.ProjectManagement
{
    public class RgbConverter : JsonConverter<Rgb[]>
    {
        public override void WriteJson(JsonWriter writer, Rgb[] value, JsonSerializer serializer)
        {
            var data = ColorUtils.RgbArrayToBytes(value);
            var json = Convert.ToBase64String(data);
            writer.WriteValue(json);
        }

        public override Rgb[] ReadJson(JsonReader reader, Type objectType, Rgb[] existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var json = reader.Value as string ?? "";
            var data = Convert.FromBase64String(json);
            return ColorUtils.BytesToRgbArray(data);
        }
    }
}