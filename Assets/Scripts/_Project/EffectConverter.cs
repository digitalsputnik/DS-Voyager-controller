using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using VoyagerController.Effects;

namespace VoyagerController.ProjectManagement
{
    public class EffectConverter : JsonConverter<Effect>
    {
        private readonly string _videosPath;
        
        public EffectConverter(string videosPath)
        {
            _videosPath = videosPath;
        }
        
        public override void WriteJson(JsonWriter writer, Effect value, JsonSerializer serializer)
        {
            switch (value)
            {
                case VideoEffect video:
                    var data = new EffectData();
                    data.Name = video.Name;
                    data.Settings = video.Settings;
                    data.Type = EffectType.Video;
                    writer.WriteRawValue(JsonConvert.SerializeObject(data, Formatting.Indented));
                    break;
            }
        }

        public override Effect ReadJson(JsonReader reader, Type objectType, Effect existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var json = JObject.Load(reader).ToString();
            var raw = JsonConvert.DeserializeObject<EffectData>(json ?? "");

            switch (raw.Type)
            {
                case EffectType.Video:
                    var video = EffectManager.GetEffectWithName<VideoEffect>(raw.Name);
                    
                    if (video == null)
                    {
                        var path = Path.Combine(_videosPath, raw.Name);
                        Debug.Log(path);
                        VideoEffectLoader.LoadVideoEffect(path, effect =>
                        {
                            effect.Settings = raw.Settings;
                        });
                    }
                    else
                    {
                        video.Settings = raw.Settings;
                    }
                    
                    break;
            }
            
            return null;
        }
    }
    
    [Serializable]
    public class EffectData
    {
        public string Name { get; set; }
        public EffectSettings Settings { get; set; }
        public EffectType Type { get; set; }
    }

    public enum EffectType
    {
        None,
        Video
    }
}