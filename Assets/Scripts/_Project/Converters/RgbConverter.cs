using System;
using System.Text;
using DigitalSputnik.Colors;
using Newtonsoft.Json;

namespace VoyagerController.ProjectManagement
{
    public class RgbConverter : JsonConverter<Rgb[]>
    {
        public override void WriteJson(JsonWriter writer, Rgb[] value, JsonSerializer serializer)
        {
            var bytes = ColorUtils.RgbArrayToBytes(value);
            var json = Encoding.Unicode.GetString(bytes);
            writer.WriteValue(json);
        }

        public override Rgb[] ReadJson(JsonReader reader, Type objectType, Rgb[] existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var json = reader.Value as string;
            var bytes = Encoding.Unicode.GetBytes(json ?? "");
            return ColorUtils.BytesToRgbArray(bytes);
        }
    }
}