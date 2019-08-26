using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using VoyagerApp.Networking;
using VoyagerApp.Workspace;
using VoyagerApp.Workspace.Views;
using static VoyagerApp.Workspace.WorkspaceSaveLoad;

namespace VoyagerApp.UI.Menus
{
    public class LoadMenuItem : MonoBehaviour
    {
        [SerializeField] Text nameText  = null;
        [SerializeField] Text lampsText = null;

        string path;
        bool vm;

        public void SetPath(string path, bool vm)
        {
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new IPAddressConverter());
            settings.Converters.Add(new IPEndPointConverter());
            settings.Converters.Add(new Texture2DConverter());
            settings.TypeNameHandling = TypeNameHandling.All;
            settings.Formatting = Formatting.Indented;

            var json = File.ReadAllText(path);
            var data = JsonConvert.DeserializeObject<WorkspaceSaveData>(json, settings);
            var lamps = data.items.Where(_ => _ is LampItemSaveData).ToList();

            nameText.text = Path.GetFileNameWithoutExtension(path);
            lampsText.text = $"LAMPS: {lamps.Count}";

            this.path = path;
            this.vm = vm;
        }

        public void Load()
        {
            string prefix = "";
            if (vm) prefix = "vm/";
            WorkspaceSaveLoad.Load(prefix + Path.GetFileNameWithoutExtension(path));
            if (vm) GetComponentInParent<LoadMenu>().VideoMappingLoaded();
            GetComponentInParent<InspectorMenuContainer>().ShowMenu(null);
        }

        public void Remove()
        {
            File.Delete(path);
            Destroy(gameObject);
        }
    }
}