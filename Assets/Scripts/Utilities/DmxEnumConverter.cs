using System;
using System.Globalization;
using Newtonsoft.Json;
using VoyagerApp.Dmx;

namespace VoyagerApp.Utilities
{
    public class DmxEnumConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DmxProtocol) || objectType == typeof(DmxFormat);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is DmxProtocol)
                writer.WriteValue(value.ToString());

            if (value is DmxFormat)
                writer.WriteValue(value.ToString().ToLower());

            writer.WriteValue("ERROR");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (objectType == typeof(DmxProtocol))
            {
                Enum.TryParse((string)reader.Value, out DmxProtocol protocol);
                return protocol;
            }

            if (objectType == typeof(DmxFormat))
            {
                string value = (string)reader.Value;
                new CultureInfo("en-US").TextInfo.ToTitleCase(value);
                Enum.TryParse(value, out DmxFormat protocol);
                return protocol;
            }

            return null;
        }
    }
}