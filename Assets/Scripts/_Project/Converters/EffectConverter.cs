using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DigitalSputnik;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
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
            var data = new EffectData { Name = value.Name, Settings = value.Settings };

            switch (value)
            {
                case VideoEffect _:
                    data.Type = EffectType.Video;
                    break;
                case ImageEffect imageEffect:
                    data.Type = EffectType.Picture;

                    var pictureBytes = imageEffect.ImageTexture.EncodeToJPG();
                    var picturePath = Path.Combine(_videosPath, imageEffect.Name + ".jpg");
                    File.WriteAllBytes(picturePath, pictureBytes);
                    
                    break;
                default:
                    data.Type = EffectType.Other;
                    break;
            }
            
            writer.WriteRawValue(JsonConvert.SerializeObject(data, Formatting.Indented));
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
                case EffectType.Picture:
                    var picture = EffectManager.GetEffectWithName<ImageEffect>(raw.Name);

                    if (picture == null)
                    {
                        var path = Path.Combine(_videosPath, raw.Name + ".jpg");
                        var image = new ImageEffect(path) { Meta = { Timestamp = TimeUtils.Epoch }};
                        EffectManager.AddEffect(image);
                    }
                    else
                    {
                        picture.Settings = raw.Settings;
                    }

                    break;
                default:
                    var effect = EffectManager.GetEffectWithName(raw.Name);

                    if (effect != null)
                    {
                        effect.Settings = raw.Settings;
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
        Other,
        Video,
        Picture,
    }
}