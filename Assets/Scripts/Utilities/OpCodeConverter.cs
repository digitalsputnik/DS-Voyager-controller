using System;
using System.Linq;
using Newtonsoft.Json;
using VoyagerApp.Networking.Voyager;

namespace VoyagerApp.Utilities
{
    public class OpCodeConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            OpCode enumValue = (OpCode)value;
            string stringValue = enumValue.ToString();
            string result = string.Concat(stringValue.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString())).ToLower();
            writer.WriteValue(result);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var enumString = (string)reader.Value;

            return Enum.Parse(typeof(OpCode), enumString.Replace("_", ""), true);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }
    }
}