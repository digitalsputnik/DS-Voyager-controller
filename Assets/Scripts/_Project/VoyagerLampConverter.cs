using System;
using DigitalSputnik.Voyager;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VoyagerController.ProjectManagement
{
    public class VoyagerLampConverter : JsonConverter<VoyagerLamp>
    {
        public override void WriteJson(JsonWriter writer, VoyagerLamp value, JsonSerializer serializer)
        {
            var data = new VoyagerData
            {
                Serial = value.Serial,
                PixelCount = value.PixelCount,
                Type = value.Type
            };
            
            writer.WriteRawValue(JsonConvert.SerializeObject(data, Formatting.Indented));
        }

        public override VoyagerLamp ReadJson(JsonReader reader, Type objectType, VoyagerLamp existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var json = JObject.Load(reader).ToString();
            var data = JsonConvert.DeserializeObject<VoyagerData>(json);

            var voyager = new VoyagerLamp(null)
            {
                Serial = data.Serial,
                PixelCount = data.PixelCount, 
                Type = data.Type
            };

            return voyager;
        }

        private class VoyagerData
        {
            public string Serial { get; set; }
            public int PixelCount { get; set; }
            public VoyagerType Type { get; set; }
        }
    }
}