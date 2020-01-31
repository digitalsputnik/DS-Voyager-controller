using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using VoyagerApp.Effects;
using VoyagerApp.Lamps;
using VoyagerApp.Utilities;

namespace VoyagerApp.Videos
{
    [Serializable]
    public class EffectMappingSettings
    {
        public string effect;
        public string[] lamps;

        [JsonIgnore]
        static string path => Path.Combine(
            FileUtils.ProjectPath,
            "to_effect_mapping.tmp"
        );

        public EffectMappingSettings() { }

        public EffectMappingSettings(List<Lamp> lamps, Effect effect)
        {
            List<string> tempLamps = new List<string>();
            lamps.ForEach(l => tempLamps.Add(l.serial));

            this.effect = effect == null ? "" : effect.id;
            this.lamps = tempLamps.ToArray();
        }

        public void Save()
        {
            var json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(path, json);
        }

        public static EffectMappingSettings Load()
        {
            if (!File.Exists(path)) return null;

            string json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<EffectMappingSettings>(json);
        }
    }
}