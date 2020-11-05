using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DigitalSputnik;
using DigitalSputnik.Voyager;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using VoyagerController.Effects;
using VoyagerController.Rendering;
using VoyagerController.Workspace;

namespace VoyagerController.ProjectManagement
{
    // The data should have two sections:
    // - Lamp information
    // - Workspace layout

    public class Project : MonoBehaviour
    {
        public const string VIDEOS_DIRECTORY = "videos";
        private const string PROJECTS_DIRECTORY = "projects";
        private const string PROJECT_FILE = "project.dsprj";

        private static Project _instance;
        
        [SerializeField] private string _version;

        private void Awake() => _instance = this;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.L))
                Load("test");
        }

        #region Save
        public static void Save(string name, bool hide = false)
        {
            var path = SetupFolderStructure(name);
            var project = ConstructProjectDataClass(hide);
            var videos = Path.Combine(path, VIDEOS_DIRECTORY);
            var settings = ConstructJsonSettings(videos);
            
            CopyVideosToProject(path);

            var json = JsonConvert.SerializeObject(project, settings);
            var file = Path.Combine(path, PROJECT_FILE);
            
            json = JToken.Parse(json).ToString(); // To make json pretty! :)

            File.WriteAllText(file, json, Encoding.UTF8);
        }

        private static string SetupFolderStructure(string name)
        {
            var project = GetProjectPath(name);
            var videos = Path.Combine(project, VIDEOS_DIRECTORY);

            Directory.CreateDirectory(project);
            Directory.CreateDirectory(videos);
            
            return project;
        }

        private static void CopyVideosToProject(string projectPath)
        {
            var videosPath = Path.Combine(projectPath, VIDEOS_DIRECTORY);

            foreach (var videoEffect in EffectManager.GetEffects<VideoEffect>())
            {
                if (IsEffectPreset(videoEffect)) continue;
                if (videoEffect.Video.Path.StartsWith(videosPath)) continue;

                var name = Path.GetFileName(videoEffect.Video.Path);
                var destination = Path.Combine(videosPath, name);
                
                File.Copy(videoEffect.Video.Path, destination);

                videoEffect.Video.Path = destination;
            }
        }

        private static ProjectData ConstructProjectDataClass(bool hide)
        {
            return new ProjectData
            {
                Hide = hide,
                Version = _instance._version,
                Lamps = LampManager.Instance.GetLampsOfType<VoyagerLamp>().ToArray(),
                Effects = EffectManager.GetEffects().ToArray(),
                LampMetadata = Metadata.GetAll()
            };
        }
        #endregion

        #region Load
        public static void Load(string name)
        {
            var path = GetProjectPath(name);
            var file = Path.Combine(path, PROJECT_FILE);
            var videos = Path.Combine(path, VIDEOS_DIRECTORY);
            var json = File.ReadAllText(file);
            var settings = ConstructJsonSettings(videos);
            var project = JsonConvert.DeserializeObject<ProjectData>(json, settings);
            
            ClearWorkspace();
            ClearEffects();
            
            LoadProjectData(project);
        }

        private static void ClearWorkspace()
        {
            var items = WorkspaceManager.GetItems().ToArray();
            foreach (var item in items)
                WorkspaceManager.RemoveItem(item);
        }

        private static void ClearEffects()
        {
            var effects = EffectManager.GetEffects().ToArray();
            foreach (var effect in effects)
            {
                if (!IsEffectPreset(effect))
                    EffectManager.RemoveEffect(effect);
            }
        }

        private static void LoadProjectData(ProjectData data)
        {
            WorkspaceManager.Clear();
            VideoEffectRenderer.Stop();

            foreach (var voyagerLamp in data.Lamps)
            {
               if (LampManager.Instance.GetLampWithSerial<VoyagerLamp>(voyagerLamp.Serial) == null)
                   LampManager.Instance.AddLamp(voyagerLamp);
            }

            foreach (var dataPair in data.LampMetadata)
            {
                var serial = dataPair.Key;
                var metadata = dataPair.Value;
                Metadata.SetMetadata(serial, metadata);

                if (metadata.InWorkspace)
                {
                    var voyager = LampManager.Instance.GetLampWithSerial<VoyagerLamp>(serial);
                    WorkspaceManager.InstantiateItem<VoyagerItem>(voyager);
                }
            }
        }
        #endregion

        private static string GetProjectPath(string name)
        {
            var persistent = Application.persistentDataPath;
            var projects = Path.Combine(persistent, PROJECTS_DIRECTORY);
            return Path.Combine(projects, name);
        }
        
        private static JsonSerializerSettings ConstructJsonSettings(string videosPath)
        {
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new EffectConverter(videosPath));
            settings.Converters.Add(new RgbConverter());
            settings.Converters.Add(new LampMetadataConverter());
            settings.Converters.Add(new VoyagerLampConverter());
            settings.NullValueHandling = NullValueHandling.Ignore;
            settings.Formatting = Formatting.Indented;
            return settings;
        }

        private static bool IsEffectPreset(Effect video)
        {
            return EffectManager.Presets.Any(p => Path.GetFileNameWithoutExtension(video.Name) == p);
        }
    }

    [Serializable]
    public struct ProjectData
    {
        public bool Hide { get; set; }
        public string Version { get; set; }
        public VoyagerLamp[] Lamps { get; set; }
        public Effect[] Effects { get; set; }
        public Dictionary<string, LampMetadata> LampMetadata { get; set; }
    }
}