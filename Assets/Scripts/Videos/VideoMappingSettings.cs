using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using VoyagerApp.Lamps;
using VoyagerApp.Utilities;

namespace VoyagerApp.Videos
{
    [Serializable]
    public class VideoMappingSettings
    {
        public string video;
        public string[] lamps;

        [JsonIgnore]
        static string path => Path.Combine(
            FileUtils.ProjectPath,
            "to_video_mapping.tmp"
        );

        public VideoMappingSettings() { }

        public VideoMappingSettings(List<Lamp> lamps, Video video)
        {
            List<string> tempLamps = new List<string>();
            lamps.ForEach(l => tempLamps.Add(l.serial));

            this.video = video == null ? "" : video.hash;
            this.lamps = tempLamps.ToArray();
        }

        public void Save()
        {
            var json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(path, json);
        }

        public static VideoMappingSettings Load()
        {
            if (!File.Exists(path)) return null;

            string json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<VideoMappingSettings>(json);
        }
    }
}