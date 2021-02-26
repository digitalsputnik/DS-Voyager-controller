using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VoyagerController.Effects;

namespace VoyagerController.ProjectManagement
{
    public class EffectConverter : JsonConverter<Effect>, IDisposable
    {
        private readonly string _videosPath;
        private readonly Dictionary<string, EffectSettings> _waitingList = new Dictionary<string, EffectSettings>();
        
        public EffectConverter(string videosPath)
        {
            _videosPath = videosPath;
            EffectManager.OnEffectAdded += OnEffectAdded;
        }

        private void OnEffectAdded(Effect effect)
        {
            if (!_waitingList.ContainsKey(effect.Name)) return;
            
            effect.Settings = _waitingList[effect.Name];
            _waitingList.Remove(effect.Name);
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
                        if (EffectManager.Presets.Contains(raw.Name))
                        {
                            _waitingList.Add(raw.Name, raw.Settings);
                        }
                        else
                        {
                            var path = Path.Combine(_videosPath, raw.Name + ".mp4");
                            VideoEffectLoader.LoadVideoEffect(path, effect =>
                            {
                                effect.Settings = raw.Settings;
                            });
                        }
                    }
                    else
                    {
                        video.Settings = raw.Settings;
                    } 
                    

                    break;
            }
            
            return null;
        }
        
        public void Dispose()
        {
            EffectManager.OnEffectAdded -= OnEffectAdded;
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