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
        private readonly Dictionary<string, EffectData> _waitingList = new Dictionary<string, EffectData>();
        
        public EffectConverter(string videosPath)
        {
            _videosPath = videosPath;
            EffectManager.OnEffectAdded += OnEffectAdded;
        }

        private void OnEffectAdded(Effect effect)
        {
            if (!_waitingList.ContainsKey(effect.Name)) return;
            
            effect.Settings = _waitingList[effect.Name].Settings;

            if (effect is VideoEffect video)
                video.Video.Fps = _waitingList[effect.Name].Fps;
            
            _waitingList.Remove(effect.Name);
        }

        public override void WriteJson(JsonWriter writer, Effect value, JsonSerializer serializer)
        {
            var data = new EffectData { Name = value.Name, Settings = value.Settings };

            switch (value)
            {
                case VideoEffect video:
                    data.Type = EffectType.Video;
                    data.Fps = video.Video.Fps;
                    break;
                case ImageEffect imageEffect:
                    data.Type = EffectType.Picture;

                    var pictureBytes = imageEffect.ImageTexture.EncodeToJPG();
                    var picturePath = Path.Combine(_videosPath, imageEffect.Name + ".jpg");
                    File.WriteAllBytes(picturePath, pictureBytes);
                    
                    break;
                case SyphonEffect syphon:
                    data.Delay = syphon.Delay;
                    data.Type = EffectType.Stream;
                    break;
                case SpoutEffect spout:
                    data.Delay = spout.Delay;
                    data.Type = EffectType.Stream;
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
                        if (EffectManager.VideoPresets.Contains(raw.Name))
                        {
                            _waitingList.Add(raw.Name, raw);
                        }
                        else
                        {
                            var path = "";
                            if (Application.platform == RuntimePlatform.IPhonePlayer)
                                path = Path.Combine(_videosPath, raw.Name + ".MOV");
                            else
                                path = Path.Combine(_videosPath, raw.Name + ".mp4");

                            VideoEffectLoader.LoadVideoEffect(path, e =>
                            {
                                e.Settings = raw.Settings;
                                ((VideoEffect) e).Video.Fps = raw.Fps;
                            });
                        }
                    }
                    else
                    {
                        video.Settings = raw.Settings;
                        video.Video.Fps = raw.Fps;
                    } 
                    

                    break;
                case EffectType.Picture:
                    var picture = EffectManager.GetEffectWithName<ImageEffect>(raw.Name);

                    if (picture == null)
                    {
                        if (EffectManager.ImagePresets.Contains(raw.Name))
                        {
                            _waitingList.Add(raw.Name, raw);
                        }
                        else
                        {
                            var path = Path.Combine(_videosPath, raw.Name + ".jpg");
                            var image = new ImageEffect(path) { Meta = { Timestamp = TimeUtils.Epoch } };
                            EffectManager.AddEffect(image);
                        }
                    }
                    else
                    {
                        picture.Settings = raw.Settings;
                    }

                    break;
                case EffectType.Stream:
                    var syphon = EffectManager.GetEffectWithName<SyphonEffect>(raw.Name);
                    if (syphon != null)
                    {
                        syphon.Delay = raw.Delay;
                        syphon.Settings = raw.Settings;
                    }

                    var spout = EffectManager.GetEffectWithName<SpoutEffect>(raw.Name);
                    if (spout != null)
                    {
                        spout.Delay = raw.Delay;
                        spout.Settings = raw.Settings;
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
        public double Fps { get; set; }
        public double Delay { get; set; }
    }

    public enum EffectType
    {
        Other,
        Video,
        Picture,
        Stream,
    }
}