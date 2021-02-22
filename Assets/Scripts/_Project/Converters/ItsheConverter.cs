using System;
using DigitalSputnik.Colors;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VoyagerController.ProjectManagement
{
    public class ItsheConverter : JsonConverter<Itshe>
    {
        public override void WriteJson(JsonWriter writer, Itshe value, JsonSerializer serializer)
        {
            var data = new [] { value.I, value.T, value.S, value.H, value.E };
            writer.WriteRawValue(JsonConvert.SerializeObject(data));
        }

        public override Itshe ReadJson(JsonReader reader, Type objectType, Itshe existingValue, bool hasExistingValue, JsonSerializer serializer)
        { 
            var json = JArray.Load(reader).ToString();
            var values = JsonConvert.DeserializeObject<float[]>(json);
            return new Itshe(values[0], values[1], values[2], values[3], values[4]);
        }
    }
}