using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VoyagerController.ProjectManagement
{
    public class EffectMappingConverter : JsonConverter<EffectMapping>
    {
        public override void WriteJson(JsonWriter writer, EffectMapping value, JsonSerializer serializer)
        {
            writer.WriteRawValue(JsonConvert.SerializeObject(value.Positions));
        }

        public override EffectMapping ReadJson(JsonReader reader, Type objectType, EffectMapping existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var json = JArray.Load(reader).ToString();
            return new EffectMapping { Positions = JsonConvert.DeserializeObject<float[]>(json) };
        }
    }
}