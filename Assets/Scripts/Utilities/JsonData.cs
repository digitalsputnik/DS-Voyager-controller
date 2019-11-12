using System.Text;
using Newtonsoft.Json;

namespace VoyagerApp.Utilities
{
    public class JsonData<T>
    {
        public static T FromData(byte[] data)
        {
            string json = Encoding.UTF8.GetString(data);
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static T FromData(byte[] data, JsonSerializerSettings settings)
        {
            string json = Encoding.UTF8.GetString(data);
            return JsonConvert.DeserializeObject<T>(json, settings);
        }

        public byte[] ToData()
        {
            string json = JsonConvert.SerializeObject(this);
            return Encoding.UTF8.GetBytes(json);
        }

        public byte[] ToData(JsonSerializerSettings settings)
        {
            string json = JsonConvert.SerializeObject(this, settings);
            return Encoding.UTF8.GetBytes(json);
        }
    }
}