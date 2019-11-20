using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VoyagerApp.Projects
{
    public class ProjectParser2_0 : IProjectParser
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
                video.guid = (string)videoToken["hash"];
                video.url = (string)videoToken["path"];
                video.frames = (long)videoToken["frames"];
                video.fps = (int)videoToken["fps"];
                videos[i] = video;
            }

            int lampsLength = jobj["lamps"].Children().Count();
            var lamps = new Lamp[lampsLength];
            for (int i = 0; i < lampsLength; i++)
            {
                var lampToken = jobj["lamps"][i];
                var itshToken = lampToken["itshe"];
                var mappingToken = lampToken["mapping"];
                var videoToken = lampToken["video"];

                Lamp lamp = new Lamp();

                lamp.serial = (string)lampToken["serial"];
                lamp.length = (int)lampToken["pixels"];
                lamp.address = (string)lampToken["address"];
                lamp.buffer = JsonConvert.DeserializeObject<byte[][]>(lampToken["buffer"]?["framesToBuffer"]?.ToString()) ?? null;

                if (videoToken.Type != JTokenType.Null)
                {
                    lamp.video = (string)((JArray)jobj["videos"])
                        .First(v => (int)v["$id"] == (int)videoToken["$ref"])["hash"];
                }

                lamp.itsh = new float[]
                {
                    (float)itshToken["i"],
                    (float)itshToken["t"],
                    (float)itshToken["s"],
                    (float)itshToken["h"],
                    (float)itshToken["e"]
                };

                if (mappingToken.Type != JTokenType.Null)
                {
                    lamp.mapping = new float[]
                    {
                        (float)mappingToken["x1"],
                        (float)mappingToken["y1"],
                        (float)mappingToken["x2"],
                        (float)mappingToken["y2"]
                    };
                }

                lamps[i] = lamp;
            }

            int itemsLenght = jobj["items"].Children().Count();
            var items = new Item[itemsLenght];
            for (int i = 0; i < itemsLenght; i++)
            {
                var itemToken = jobj["items"][i];
                string type = (string)itemToken["$type"];

                switch (type)
                {
                    case "VoyagerApp.Workspace.Views.LampItemSaveData, Assembly-CSharp":
                        var lampItem = new LampItem();
                        lampItem.type = "voyager_lamp";
                        lampItem.scale = (float)itemToken["scale"];
                        lampItem.rotation = (float)itemToken["rotation"];

                        lampItem.serial = (string)((JArray)jobj["lamps"])
                            .First(l => (int)l["$id"] == (int)itemToken["lamp"]["$ref"])["serial"];

                        lampItem.position = new float[]
                        {
                            (float)itemToken["x"],
                            (float)itemToken["y"]
                        };

                        items[i] = lampItem;
                        break;
                    case "VoyagerApp.Workspace.Views.PictureItemSaveData, Assembly-CSharp":
                        var pictureItem = new PictureItem();
                        pictureItem.type = "picture";
                        pictureItem.width = 4;
                        pictureItem.height = 4;
                        pictureItem.data = (byte[])itemToken["image"];
                        pictureItem.order = (int)itemToken["queueIndex"];
                        pictureItem.scale = (float)itemToken["scale"];
                        pictureItem.rotation = (float)itemToken["rotation"];

                        pictureItem.position = new float[]
                        {
                            (float)itemToken["x"],
                            (float)itemToken["y"]
                        };

                        items[i] = pictureItem;
                        break;
                }
            }

            var camera = new float[]
            {
                (float)jobj["camPositionX"],
                (float)jobj["camPositionY"],
                (float)jobj["camSize"]
            };

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
