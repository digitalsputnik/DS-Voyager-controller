using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VoyagerApp.Projects
{
    public class ProjectParser2_1 : IProjectParser
    {
        public ProjectSaveData Parse(string json)
        {
            JObject jobj = JObject.Parse(json);

            string version = (string)jobj["version"];

            int videosLength = jobj["videos"].Children().Count();
            var videos = new Video[videosLength];
            for (int i = 0; i < videosLength; i++)
            {
                var videoToken = jobj["videos"][i];
                Video video = new Video();
                video.guid = (string)videoToken["guid"];
                video.url = (string)videoToken["url"];
                video.frames = (long)videoToken["frames"];
                video.fps = (int)videoToken["fps"];
                videos[i] = video;
            }

            int lampsLength = jobj["lamps"].Children().Count();
            var lamps = new Lamp[lampsLength];
            for (int i = 0; i < lampsLength; i++)
            {
                var lampToken = jobj["lamps"][i];
                Lamp lamp = new Lamp();
                lamp.serial = (string)lampToken["serial"];
                lamp.length = (int)lampToken["length"];
                lamp.video = (string)lampToken["video"];
                lamp.address = (string)lampToken["address"];
                lamp.itsh = ((JArray)lampToken["itsh"]).Select(m => (float)m).ToArray();
                lamp.mapping = ((JArray)lampToken["mapping"]).Select(m => (float)m).ToArray();
                lamp.buffer = JsonConvert.DeserializeObject<byte[][]>(lampToken["buffer"].ToString());
                lamps[i] = lamp;
            }

            int itemsLenght = jobj["items"].Children().Count();
            var items = new Item[itemsLenght];
            for (int i = 0; i < itemsLenght; i++)
            {
                var itemToken = jobj["items"][i];
                string type = (string)itemToken["type"];

                switch (type)
                {
                    case "voyager_lamp":
                        var lampItem = new LampItem();
                        lampItem.type = type;
                        lampItem.serial = (string)itemToken["serial"];
                        lampItem.position = ((JArray)itemToken["position"]).Select(p => (float)p).ToArray();
                        lampItem.scale = (float)itemToken["scale"];
                        lampItem.rotation = (float)itemToken["rotation"];
                        items[i] = lampItem;
                        break;

                    case "picture":
                        var pictureItem = new PictureItem();
                        pictureItem.type = type;
                        pictureItem.width = (int)itemToken["width"];
                        pictureItem.height = (int)itemToken["height"];
                        pictureItem.data = (byte[])itemToken["data"];
                        pictureItem.order = (int)itemToken["order"];
                        pictureItem.position = ((JArray)itemToken["position"]).Select(p => (float)p).ToArray();
                        pictureItem.scale = (float)itemToken["scale"];
                        pictureItem.rotation = (float)itemToken["rotation"];
                        items[i] = pictureItem;
                        break;
                }
            }

            float[] camera = ((JArray)jobj["camera"]).Select(d => (float)d).ToArray();

            return new ProjectSaveData
            {
                version = version,
                videos = videos,
                lamps = lamps,
                items = items,
                camera = camera
            };
        }
    }
}
