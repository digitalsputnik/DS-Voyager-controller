using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using VoyagerApp.Networking;
using VoyagerApp.UI;
using VoyagerApp.Utilities;

namespace VoyagerApp.Workspace
{
    public class WorkspaceSaveLoad : MonoBehaviour
    {
        const string folder = "workspaces";
        const string tmpFolder = "tmp";

        static JsonSerializerSettings settings;

        void Awake()
        {
            settings = new JsonSerializerSettings();
            settings.Converters.Add(new IPAddressConverter());
            settings.Converters.Add(new IPEndPointConverter());
            settings.Converters.Add(new Texture2DConverter());
            settings.TypeNameHandling = TypeNameHandling.All;
            settings.Formatting = Formatting.Indented;

            string path = Path.Combine(Application.persistentDataPath, folder, tmpFolder);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        }

        public static void Save(string name, WorkspaceItemView[] items)
        {
            WorkspaceSelection.instance.Clear();

            name += ".ws";

            WorkspaceSaveData data = CreateData(items);
            string json = JsonConvert.SerializeObject(data, settings);
            var path = Path.Combine(Application.persistentDataPath, folder, name);

            File.WriteAllText(path, json);
        }

        public static void Load(string name)
        {
            WorkspaceManager.instance.Clear();

            name += ".ws";

            var path = Path.Combine(Application.persistentDataPath, folder, name);
            var json = File.ReadAllText(path);
            var data = JsonConvert.DeserializeObject<WorkspaceSaveData>(json, settings);

            foreach (var item in data.items) item.Load();

            foreach (var item in data.items)
            {
                if (item.parentguid != "")
                {
                    var itm = WorkspaceManager.instance.GetItem(item.guid);
                    var parent = WorkspaceManager.instance.GetItem(item.parentguid);
                    itm.SetParent(parent);
                }
            }

            Camera cam = Camera.main;

            Vector3 camPos = new Vector2(data.camX, data.camY);
            camPos.z = cam.transform.position.z;

            cam.transform.position = camPos;
            cam.orthographicSize = data.camSize;
        }

        public static WorkspaceSaveData CreateData(WorkspaceItemView[] views)
        {
            var items = new WorkspaceItemSaveData[views.Length];
            for (int i = 0; i < views.Length; i++)
                items[i] = views[i].ToData();

            Camera cam = Camera.main;

            return new WorkspaceSaveData
            {
                items = items,
                camX = cam.transform.position.x,
                camY = cam.transform.position.y,
                camSize = cam.orthographicSize
            };
        }

        public class WorkspaceSaveData
        {
            public WorkspaceItemSaveData[] items;

            public float camX;
            public float camY;
            public float camSize;
        }
    }
}
