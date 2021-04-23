using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DigitalSputnik;
using DigitalSputnik.Voyager;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using VoyagerController.Effects;
using VoyagerController.Rendering;
using VoyagerController.UI;
using VoyagerController.Workspace;

namespace VoyagerController.ProjectManagement
{
    public class Project : MonoBehaviour
    {
        public const string VIDEOS_DIRECTORY = "videos";
        private const string PROJECTS_DIRECTORY = "projects";
        public const string PROJECT_FILE = "project.dsprj";

        private static Project _instance;
        
        [SerializeField] private string _version;

        private void Awake() => _instance = this;

        #region New

        public static void New()
        {
            ClearWorkspace();
            ClearEffects();
            WorkspaceManager.Clear();
            VideoEffectRenderer.Stop();
        }

        #endregion

        #region Save
        public static ProjectData Save(string name, bool hide = false)
        {
            var path = SetupFolderStructure(name);

            try
            {
                var project = ConstructProjectDataClass(hide);
                var videos = Path.Combine(path, VIDEOS_DIRECTORY);
                var settings = ConstructJsonSettings(videos);

                CopyVideosToProject(path);

                var json = JsonConvert.SerializeObject(project, settings);
                var file = Path.Combine(path, PROJECT_FILE);

                json = JToken.Parse(json).ToString(); // To make json pretty! :)

                File.WriteAllText(file, json, Encoding.UTF8);

                return project;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);

                if (Directory.Exists(path))
                    Directory.Delete(path, true);

                DialogBox.Show(
                    "ERROR",
                    $"Unable to save project. " +
                    $"Following error occurred:\n{ex.Message}",
                    new string[] { "CANCEL", "OK" },
                    new Action[] { null, null });

                return null;
            }
        }

        private static string SetupFolderStructure(string name)
        {
            var project = Path.Combine(ProjectsDirectory, name);
            var videos = Path.Combine(project, VIDEOS_DIRECTORY);

            if (!Directory.Exists(project))
                Directory.CreateDirectory(project);
            if (!Directory.Exists(videos))
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
                AppVersion = Application.version,
                Time = DateTime.Now,
                Lamps = LampManager.Instance.GetLampsOfType<VoyagerLamp>().ToArray(),
                Effects = EffectManager.GetEffects().ToArray(),
                LampMetadata = Metadata.GetPairs<LampData>(),
                PictureMetadata = Metadata.GetPairs<PictureData>()
            };
        }
        #endregion

        #region Load
        public static ProjectData Load(string name)
        {
            var path = Path.Combine(ProjectsDirectory, name);

            if (!Directory.Exists(path)) 
            {
                Debug.Log("Directory doesn't exist" + path);
                return null;
            }

            var file = Path.Combine(path, PROJECT_FILE);
            var videos = Path.Combine(path, VIDEOS_DIRECTORY);
            var json = File.ReadAllText(file);
            var settings = ConstructJsonSettings(videos);
            var project = JsonConvert.DeserializeObject<ProjectData>(json, settings);
            
            ClearWorkspace();
            ClearEffects();
            
            LoadProjectData(project);

            return project;
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
                var existing = LampManager.Instance.GetLampWithSerial<VoyagerLamp>(voyagerLamp.Serial);

                if (existing == null)
                {
                    LampManager.Instance.AddLamp(voyagerLamp);
                }
                else
                {
                    existing.DmxModeEnabled = voyagerLamp.DmxModeEnabled;
                    existing.DmxSettings = voyagerLamp.DmxSettings;
                }
            }

            foreach (var dataPair in data.LampMetadata)
            {
                var serial = dataPair.Key;
                var metadata = dataPair.Value;
                
                Metadata.Set(serial, metadata);

                if (metadata.InWorkspace)
                {
                    var voyager = LampManager.Instance.GetLampWithSerial<VoyagerLamp>(serial);
                    WorkspaceManager.InstantiateItem<VoyagerItem>(voyager);
                }
            }

            foreach (var dataPair in data.PictureMetadata)
            {
                var id = dataPair.Key;
                var metadata = dataPair.Value;

                if (!Metadata.Contains(id))
                    Metadata.Add<PictureData>(id);
                Metadata.Set(id, metadata);

                var texture = Metadata.Get<PictureData>(id).Texture;
                WorkspaceManager.InstantiateItem<PictureItem>(texture, id);
            }
        }
        #endregion

        #region Export

        public static void Export(string save, Action<bool, string> onPacked)
        {
            string tempPath = Path.Combine(ProjectsDirectory, save);
            string projPath = Path.Combine(tempPath, PROJECT_FILE);

            try
            {
                var data = Save(save, true);

                if (data == null)
                    throw new Exception("Something went wrong while saving the project");

                CompressProjectDirectory(tempPath, ProjectsDirectory, save, onPacked);
            }
            catch(Exception ex)
            {
                Debug.LogError(ex);
            }
        }

        private static async void CompressProjectDirectory(string sourcePath, string path, string name, Action<bool, string> onPacked)
        {
            string finalPath = path + "/" + name + ".zip";

            int[] dirProgress = new int[1];
            var fileCount = Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories).Count();

            await Task.Run(() => lzip.compressDir(sourcePath, 0, finalPath, false, dirProgress));

            //(fileCount != dirProgress[0]) - Filecount is the number of files in source folder and dirProgress should the number of files currently compressed

            if (Directory.Exists(sourcePath))
                Directory.Delete(sourcePath, true);

            onPacked?.Invoke(true, finalPath);
        }

        #endregion

        #region Import

        public static async void Import(string file, Action<bool, string> onUnpacked)
        {
            string name = Path.GetFileNameWithoutExtension(file);
            string importPath = ProjectsDirectory + "/" + name;

            if (!Directory.Exists(importPath))
                Directory.CreateDirectory(importPath);

            bool failed = true;

            int[] progress = new int[1];
            ulong[] progress2 = new ulong[1];

            try
            {
                var fileInfo = lzip.getFileInfo(file);
                await Task.Run(() => lzip.decompress_File(file, importPath, progress, null, progress2));
                var progressPercentage = (float)(fileInfo / progress2[0]) * 100; //This should be the progress percentage
                failed = false;
            }
            catch(Exception ex)
            {
                Debug.LogError(ex);
            }

            if (failed)
                onUnpacked?.Invoke(false, null);
            else
                onUnpacked?.Invoke(true, importPath);
        }

        #endregion

        public static JsonSerializerSettings ConstructJsonSettings(string videosPath)
        {
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new EffectConverter(videosPath));
            settings.Converters.Add(new RgbConverter());
            settings.Converters.Add(new LampMetadataConverter());
            settings.Converters.Add(new VoyagerLampConverter());
            settings.Converters.Add(new Texture2DConverter());
            settings.NullValueHandling = NullValueHandling.Ignore;
            settings.Formatting = Formatting.Indented;
            return settings;
        }

        private static bool IsEffectPreset(Effect video)
        {
            return EffectManager.VideoPresets.Any(p => Path.GetFileNameWithoutExtension(video.Name) == p) && EffectManager.ImagePresets.Any(p => Path.GetFileNameWithoutExtension(video.Name) == p);
        }

        public static string ProjectsDirectory
        {
            get
            {
                var persistent = Application.persistentDataPath;
                var projects = Path.Combine(persistent, PROJECTS_DIRECTORY);

                if (!Directory.Exists(projects))
                    Directory.CreateDirectory(projects);

                return projects;
            }
        }
    }

    [Serializable]
    public class ProjectData
    {
        public bool Hide { get; set; }
        public string Version { get; set; }
        public string AppVersion { get; set; }
        public DateTime Time { get; set; }
        public VoyagerLamp[] Lamps { get; set; }
        public Effect[] Effects { get; set; }
        public Dictionary<string, LampData> LampMetadata { get; set; }
        public Dictionary<string, PictureData> PictureMetadata { get; set; }
    }
}