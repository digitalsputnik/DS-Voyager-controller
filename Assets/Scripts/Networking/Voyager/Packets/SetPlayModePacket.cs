using System;
using System.Linq;
using Newtonsoft.Json;
using VoyagerApp.Utilities;

namespace VoyagerApp.Networking.Voyager
{
    public class SetPlayModePacket : Packet
    {
        [JsonProperty("mode")]
        public PlaybackMode mode;

        public SetPlayModePacket(PlaybackMode mode) : base(OpCode.SetPlayMode)
        {
            this.mode = mode;
        }
    }

    [Serializable]
    [JsonConverter(typeof(PlaybackModeConverter))]
    public enum PlaybackMode { Play, Pause, Stop }

    public class PlaybackModeConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            PlaybackMode enumValue = (PlaybackMode)value;
            string stringValue = enumValue.ToString();
            string result = string.Concat(stringValue.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString())).ToLower();
            writer.WriteValue(result);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var enumString = (string)reader.Value;

            return Enum.Parse(typeof(PlaybackMode), enumString.Replace("_", ""), true);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }
    }
}