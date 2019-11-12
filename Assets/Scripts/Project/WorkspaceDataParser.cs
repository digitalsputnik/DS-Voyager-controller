using System.Linq;
using Newtonsoft.Json.Linq;

namespace VoyagerApp.Projects
{
    public static class WorkspaceDataParser
    {
        public static WorkspaceSaveData Parser(string json)
        {
            JObject jobj = JObject.Parse(json);

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

            return new WorkspaceSaveData
            {
                items = items,
                camera = camera
            };
        }
    }
}
