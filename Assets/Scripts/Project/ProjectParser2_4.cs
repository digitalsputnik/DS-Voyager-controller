using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VoyagerApp.Projects
{
    public class ProjectParser2_4 : IProjectParser
    {
        public string VersionString => "2.4";

        public ProjectSaveData Parse(string json)
        {
            var jsonObj = JObject.Parse(json);

            var version = (string)jsonObj["version"];

            var effectsCount = jsonObj["effects"].Children().Count();
            var effects = new Effect[effectsCount];
            for (var i = 0; i < effectsCount; i++)
            {
                var effectToken = jsonObj["effects"][i];
                var type = (string)effectToken["type"];

                var lift = (float)effectToken["lift"];
                var contrast = (float)effectToken["contrast"];
                var saturation = (float)effectToken["saturation"];
                var blur = (float)effectToken["blur"];

                switch (type)
                {
                    case "video":
                        var video = new Video();
                        video.id = (string)effectToken["id"];
                        video.file = (string)effectToken["file"];
                        video.name = (string)effectToken["name"];
                        video.frames = (long)effectToken["frames"];
                        video.fps = (int)effectToken["fps"];
                        video.type = type;
                        video.lift = lift;
                        video.contrast = contrast;
                        video.saturation = saturation;
                        video.blur = blur;
                        effects[i] = video;
                        break;
                    case "video_preset":
                        var preset = new VideoPreset();
                        preset.id = (string)effectToken["id"];
                        preset.name = (string)effectToken["name"];
                        preset.type = type;
                        preset.lift = lift;
                        preset.contrast = contrast;
                        preset.saturation = saturation;
                        preset.blur = blur;
                        effects[i] = preset;
                        break;
                    case "image":
                        var image = new Image();
                        image.id = (string)effectToken["id"];
                        image.name = (string)effectToken["name"];
                        image.data = (byte[])effectToken["data"];
                        image.type = type;
                        image.lift = lift;
                        image.contrast = contrast;
                        image.saturation = saturation;
                        image.blur = blur;
                        effects[i] = image;
                        break;
                    case "syphon":
                        var syphon = new Syphon();
                        syphon.server = (string) effectToken["server"];
                        syphon.application = (string) effectToken["application"];
                        syphon.type = type;
                        syphon.lift = lift;
                        syphon.contrast = contrast;
                        syphon.saturation = saturation;
                        syphon.blur = blur;
                        effects[i] = syphon;
                        break;
                    case "spout":
                        var spout = new Spout();
                        spout.source = (string) effectToken["source"];
                        spout.type = type;
                        spout.lift = lift;
                        spout.contrast = contrast;
                        spout.saturation = saturation;
                        spout.blur = blur;
                        effects[i] = spout;
                        break;
                }
            }

            var lampsLength = jsonObj["lamps"].Children().Count();
            var lamps = new Lamp[lampsLength];
            for (var i = 0; i < lampsLength; i++)
            {
                var lampToken = jsonObj["lamps"][i];
                Lamp lamp = new Lamp();
                lamp.serial = (string)lampToken["serial"];
                lamp.length = (int)lampToken["length"];
                lamp.effect = (string)lampToken["effect"];
                lamp.address = (string)lampToken["address"];
                lamp.itsh = ((JArray)lampToken["itsh"]).Select(m => (float)m).ToArray();
                lamp.mapping = ((JArray)lampToken["mapping"]).Select(m => (float)m).ToArray();
                lamp.buffer = JsonConvert.DeserializeObject<byte[][]>(lampToken["buffer"].ToString());
                lamps[i] = lamp;
            }

            var itemsLength = jsonObj["items"].Children().Count();
            var items = new Item[itemsLength];
            for (int i = 0; i < itemsLength; i++)
            {
                var itemToken = jsonObj["items"][i];
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

            var camera = ((JArray)jsonObj["camera"]).Select(d => (float)d).ToArray();

            return new ProjectSaveData
            {
                version = version,
                effects = effects,
                lamps = lamps,
                items = items,
                camera = camera
            };
        }
    }
}
