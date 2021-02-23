using System;
using System.Linq;
using DigitalSputnik.Colors;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VoyagerController.Effects;

namespace VoyagerController.ProjectManagement
{
    public class LampMetadataConverter : JsonConverter<LampData>
    {
        private readonly JsonSerializerSettings _settings;

        public LampMetadataConverter()
        {
            _settings = new JsonSerializerSettings();
            _settings.Converters.Add(new RgbConverter());
            _settings.Converters.Add(new EffectMappingConverter());
            _settings.Converters.Add(new ItsheConverter());
            _settings.NullValueHandling = NullValueHandling.Ignore;
            _settings.Formatting = Formatting.Indented;
        }

        public override void WriteJson(JsonWriter writer, LampData value, JsonSerializer serializer)
        {
            var data = new LampSaveData
            {
                Discovered = value.Discovered,
                Effect = value.Effect?.Name,
                Itshe = value.Itshe,
                EffectMapping = value.EffectMapping,
                InWorkspace = value.InWorkspace,
                WorkspaceMapping = value.WorkspaceMapping,
                ConfirmedFrames = value.ConfirmedFrames.Aggregate("", AddConfirmedFrame),
                PreviousStreamFrame = value.PreviousStreamFrame,
                TimeEffectApplied = value.TimeEffectApplied,
                VideoStartTime = value.VideoStartTime,
                Rendered = value.Rendered,
                FrameBuffer = value.FrameBuffer
            };
            
            writer.WriteRawValue(JsonConvert.SerializeObject(data, _settings));
        }

        private static string AddConfirmedFrame(string current, bool confirmedFrame)
        {
            return current + (confirmedFrame ? "1" : "0");
        }

        public override LampData ReadJson(JsonReader reader, Type objectType, LampData existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var json = JObject.Load(reader).ToString();
            var data = JsonConvert.DeserializeObject<LampSaveData>(json, _settings);

            if (data == null) return null;
            
            var confirmed = new bool[data.ConfirmedFrames.Length];
            for (var i = 0; i < confirmed.Length; i++)
                confirmed[i] = data.ConfirmedFrames[i] == '1';

            var meta = new LampData
            {
                Discovered = data.Discovered,
                Effect = EffectManager.GetEffectWithName(data.Effect),
                EffectMapping = data.EffectMapping,
                InWorkspace = data.InWorkspace,
                WorkspaceMapping = data.WorkspaceMapping,
                ConfirmedFrames = confirmed,
                PreviousStreamFrame = data.PreviousStreamFrame,
                TimeEffectApplied = data.TimeEffectApplied,
                VideoStartTime = data.VideoStartTime,
                Rendered = data.Rendered,
                FrameBuffer = data.FrameBuffer
            };
            
            return meta;
        }
    }

    [Serializable]
    public class LampSaveData
    {
        public double Discovered { get; set; }
        public string Effect { get; set; }
        public EffectMapping EffectMapping { get; set; }
        public bool InWorkspace { get; set; }
        public WorkspaceMapping WorkspaceMapping { get; set; }
        public Itshe Itshe { get; set; }
        public string ConfirmedFrames { get; set; }
        public Rgb[] PreviousStreamFrame { get; set; }
        public double TimeEffectApplied { get; set; }
        public double VideoStartTime { get; set; }
        public bool Rendered { get; set; }
        public Rgb[][] FrameBuffer { get; set; }
    }
}