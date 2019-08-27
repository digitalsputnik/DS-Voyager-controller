using System;
using System.Text;
using Newtonsoft.Json;
using VoyagerApp.Utilities;

namespace VoyagerApp.Networking.Packages
{
    [Serializable]
    public abstract class Packet
    {
        [JsonProperty("op_code", Order = -2)]
        public OpCode op;
        [JsonProperty("timestamp", Order = -1)]
        public double timestamp;

        protected Packet(OpCode op) => this.op = op;

        public static T Deserialize<T>(byte[] data)
        {
            string json = Encoding.UTF8.GetString(data);
            return JsonConvert.DeserializeObject<T>(json);
        }

        public byte[] Serialize()
        {
            timestamp = TimeUtils.Epoch;
            string json = JsonConvert.SerializeObject(this);
            return Encoding.UTF8.GetBytes(json);
        }
    }
}