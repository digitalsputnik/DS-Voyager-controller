using System;
using DigitalSputnik.Dmx;
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
                DmxEnabled = value.DmxModeEnabled,
                DmxSettings = value.DmxSettings,
                Type = value.Type
            };
            
            writer.WriteRawValue(JsonConvert.SerializeObject(data, Formatting.Indented));
        }

        public override VoyagerLamp ReadJson(JsonReader reader, Type objectType, VoyagerLamp existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var json = JObject.Load(reader).ToString();
            var data = JsonConvert.DeserializeObject<VoyagerData>(json);

            var voyager = new VoyagerLamp
            {
                Serial = data.Serial,
                PixelCount = data.PixelCount, 
                DmxModeEnabled = data.DmxEnabled,
                DmxSettings = data.DmxSettings,
                Type = data.Type
            };

            return voyager;
        }

        private class VoyagerData
        {
            public string Serial { get; set; }
            public int PixelCount { get; set; }
            public bool DmxEnabled { get; set; }
            public DmxSettings DmxSettings { get; set; }
            public VoyagerType Type { get; set; }
        }
    }
}