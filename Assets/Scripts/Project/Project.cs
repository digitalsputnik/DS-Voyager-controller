using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Unity.Mathematics;
using UnityEngine;
using VoyagerApp.Effects;
using VoyagerApp.Lamps;
using VoyagerApp.Lamps.Voyager;
using VoyagerApp.UI.Overlays;
using VoyagerApp.Utilities;
using VoyagerApp.Workspace;
using VoyagerApp.Workspace.Views;

namespace VoyagerApp.Projects
{
    public static class Project
    {
        public const string PROJECTS_DIRECTORY = "projects";
        public const string PROJECT_FILE = "project.dsprj";
        public const string VIDEOS = "videos";

        #region Save
        public static ProjectSaveData Save(string name)
        {
            var projPath = ProjectDirectory(name);

            try
            {
                var videos = EffectManager
                    .GetEffectsOfType<Effects.Video>()
                    .Where(v => !v.preset)
                    .ToList();

                var vidsPath = Path.Combine(projPath, VIDEOS);
                if (!Directory.Exists(vidsPath)) Directory.CreateDirectory(vidsPath);

                foreach (var video in videos)
                {
                    string vidNewPath = Path.Combine(vidsPath, video.file);

                    if (!File.Exists(vidNewPath))
                    {
                        File.Copy(video.path, vidNewPath);
                        video.path = vidNewPath;
                    }
                }

                var projectData = ProjectFactory.GetCurrentSaveData();
                var json = JsonConvert.SerializeObject(projectData, Formatting.Indented);
                var projFilePath = Path.Combine(ProjectDirectory(name), PROJECT_FILE);
                File.WriteAllText(projFilePath, json);

                return projectData;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);

                if (Directory.Exists(projPath))
                    Directory.Delete(projPath, true);

                DialogBox.Show(
                    "ERROR",
                    $"Unable to save project. " +
                    $"Following error occurred:\n{ex.Message}",
                    new string[] { "CANCEL", "OK" },
                    new Action[] { null, null });

                return null;
            }
        }

        public static void SaveWorkspace()
        {
            var data = ProjectFactory.GetCurrentWorkspaceData();
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            PlayerPrefs.SetString("workspace_temp", json);
        }

        static string[] CopyVideosIfNecessary(List<Effects.Video> videos, string name)
        {
            var path = Path.Combine(ProjectDirectory(name), VIDEOS);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            string[] paths = new string[videos.Count];
            int i = 0;

            foreach (var video in videos)
            {
                paths[i++] = video.path;

                if (!video.path.StartsWith(path, StringComparison.Ordinal))
                {
                    string newPath = Path.Combine(path, video.file);

                    try
                    {
                        if (!File.Exists(newPath))
                        {
                            File.Copy(video.path, newPath);
                            video.path = newPath;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex);
                    }
                }
            }

            return paths;
        }
        #endregion

        #region Loading
        public static ProjectSaveData Load(string name, bool positionsOnly = false)
        {
            var path = Path.Combine(ProjectsDirectory, name, PROJECT_FILE);
            
            if (!File.Exists(path)) return null;
            
            var json = File.ReadAllText(path);
            var parser = ProjectFactory.GetParser(json);
            var data = parser.Parse(json);
            
            Load(data, Path.Combine(ProjectsDirectory, name), positionsOnly);
            
            return data;
        }

        public static void LoadWorkspace()
        {
            if (!PlayerPrefs.HasKey("workspace_temp")) return;
            
            var json = PlayerPrefs.GetString("workspace_temp");
            var data = WorkspaceDataParser.Parser(json);
            LoadItems(data.items);
            LoadCamera(data.camera);
        }

        public static ProjectSaveData GetProjectData(string json)
        {
            var parser = ProjectFactory.GetParser(json);
            return parser?.Parse(json);
        }

        private static void Load(ProjectSaveData data, string path, bool positionsOnly)
        {
            LoadEffects(ref data, path);
            LoadLamps(data.lamps, positionsOnly);
            LoadItems(data.items);
            LoadCamera(data.camera);
        }

        private static void LoadEffects(ref ProjectSaveData data, string path)
        {
            EffectManager.Clear();
            
            Debug.Log(JsonConvert.SerializeObject(data, Formatting.Indented));

            var effects = data.effects;

            foreach (var effectData in effects)
            {
                switch (effectData)
                {
                    case Video videoData:
                    {
                        var existingPreset = EffectManager.Effects.FirstOrDefault(e => e.name == videoData.name);

                        if (existingPreset == null)
                        {
                            var vidPath = Path.Combine(path, VIDEOS, videoData.file);
                            if (File.Exists(vidPath))
                            {
                                var vid = VideoEffectLoader.LoadNewVideoFromPath(vidPath);
                                vid.frames = videoData.frames;
                                vid.file = videoData.file;
                                vid.fps = videoData.fps;
                                vid.id = videoData.id;
                                existingPreset = vid;
                            }
                        }

                        if (existingPreset != null)
                        {
                            existingPreset.lift = videoData.lift;
                            existingPreset.contrast = videoData.contrast;
                            existingPreset.saturation = videoData.saturation;
                            existingPreset.blur = videoData.blur;
                        }

                        break;
                    }
                    case VideoPreset videoPresetData:
                    {
                        var existingPreset = EffectManager.Effects.FirstOrDefault(e => e.name == videoPresetData.name);

                        existingPreset.lift = videoPresetData.lift;
                        existingPreset.contrast = videoPresetData.contrast;
                        existingPreset.saturation = videoPresetData.saturation;
                        existingPreset.blur = videoPresetData.blur;

                        foreach (var lamp in data.lamps)
                        {
                            if (lamp.effect == videoPresetData.id)
                                lamp.effect = existingPreset.id;
                        }

                        break;
                    }
                    case Image imageData:
                    {
                        var texture = new Texture2D(2, 2);
                        texture.LoadRawTextureData(imageData.data);

                        var image = new Effects.Image
                        {
                            id = imageData.id,
                            name = imageData.name,
                            image = texture,
                            thumbnail = texture,
                            timestamp = TimeUtils.Epoch,
                            lift = imageData.lift,
                            contrast = imageData.contrast,
                            saturation = imageData.saturation,
                            blur = imageData.blur
                        };

                        EffectManager.AddEffect(image);
                        break;
                    }
                    case Syphon syphonData:
                        if (EffectManager.Effects.FirstOrDefault(e => e is SyphonStream) is SyphonStream syphon)
                        {
                            syphon.server = syphonData.server;
                            syphon.application = syphonData.application;
                            syphon.lift = syphonData.lift;
                            syphon.contrast = syphonData.contrast;
                            syphon.saturation = syphonData.saturation;
                            syphon.blur = syphonData.blur;
                        }
                        break;
                    case Spout spoutData:
                        if (EffectManager.Effects.FirstOrDefault(e => e is SpoutStream) is SpoutStream spout)
                        {
                            spout.source = spoutData.source;
                            spout.lift = spoutData.lift;
                            spout.contrast = spoutData.contrast;
                            spout.saturation = spoutData.saturation;
                            spout.blur = spoutData.blur;
                        }
                        break;
                }
            }
        }

        private static void LoadLamps(IEnumerable<Lamp> lamps, bool positionsOnly)
        {
            foreach (var lampData in lamps)
            {
                var itsh = new Itshe(
                    lampData.itsh[0],
                    lampData.itsh[1],
                    lampData.itsh[2],
                    lampData.itsh[3],
                    lampData.itsh[4]
                );

                var mapping = EffectMapping.Default;

                if (lampData.mapping != null)
                {
                    mapping = new EffectMapping(
                        new float2(lampData.mapping[0], lampData.mapping[1]),
                        new float2(lampData.mapping[2], lampData.mapping[3]));
                }

                var lamp = LampManager.instance.GetLampWithSerial(lampData.serial);

                if (lamp == null)
                {
                    lamp = new VoyagerLamp
                    {
                        serial = lampData.serial,
                        length = lampData.length,
                        address = IPAddress.Parse(lampData.address)
                    };

                    LampManager.instance.AddLamp(lamp);
                }

                if (positionsOnly) continue;
                
                lamp.itshe = itsh;
                lamp.mapping = mapping;
            }
        }

        private static void LoadItems(IEnumerable<Item> items)
        {
            WorkspaceManager.instance.Clear();

            foreach (var item in items)
            {
                if (item is PictureItem pictureData)
                {
                    var texture = new Texture2D(pictureData.width, pictureData.height);
                    texture.LoadImage(pictureData.data);
                    texture.Apply();

                    var position = new float2(pictureData.position[0], pictureData.position[1]);
                    var obj = WorkspaceManager.instance.InstantiateItem<PictureItemView>(
                        texture,
                        position,
                        pictureData.scale,
                        pictureData.rotation
                    );

                    MainThread.Dispach(() => obj.SetOrder(pictureData.order));
                }

                if (item is LampItem lampData)
                {
                    var position = new float2(lampData.position[0], lampData.position[1]);
                    var lamp = LampManager.instance.GetLampWithSerial(lampData.serial);
                    lamp.AddToWorkspace(position, lampData.scale, lampData.rotation);
                }
            }
        }

        private static void LoadCamera(IReadOnlyList<float> camera)
        {
            var cam = Camera.main;

            var position = new Vector3(
                camera[0],
                camera[1],
                cam.transform.position.z);

            cam.orthographicSize = camera[2];
            cam.transform.position = position;
        }

        #endregion

        #region Import / Export
        public static void Export(string save, Action<bool, string> onPacked)
        {
            const string EXPORT_TEMP = "export";

            string tempPath = ProjectDirectory(EXPORT_TEMP);
            string projPath = Path.Combine(tempPath, PROJECT_FILE);
            string exportPath = Path.Combine(Application.temporaryCachePath, save + ".vcp");

            bool failed = true;

            var videos = EffectManager
                .GetEffectsOfType<Effects.Video>()
                .Where(v => !v.preset)
                .ToList();

            string[] videoPaths = new string[videos.Count];

            for (int i = 0; i < videos.Count; i++)
                videoPaths[i] = videos[i].path;

            try
            {
                var data = Save(EXPORT_TEMP);

                if (data == null)
                    throw new Exception("Something went wrong while saving the project");

                byte[] project = File.ReadAllBytes(projPath);
                byte[][] videoData = new byte[videoPaths.Length][];

                for (int i = 0; i < videoData.Length; i++)
                    videoData[i] = File.ReadAllBytes(videos[i].path);

                ProjectExport export = new ProjectExport
                {
                    project = project,
                    videos = videoData
                };

                string json = JsonConvert.SerializeObject(export, Formatting.Indented);
                byte[] bytes = Encoding.UTF8.GetBytes(json);

                string path = Path.Combine(Application.temporaryCachePath, save + ".vcp");
                File.WriteAllBytes(exportPath, bytes);

                failed = false;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
            finally
            {
                for (int i = 0; i < videos.Count; i++)
                    videos[i].path = videoPaths[i];

                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
            }

            if (failed)
                onPacked?.Invoke(false, null);
            else
                onPacked?.Invoke(true, exportPath);
        }

        public static string Import(string file)
        {
            string name = Path.GetFileNameWithoutExtension(file);

            byte[] exportBytes = File.ReadAllBytes(file);
            string exportJson = Encoding.UTF8.GetString(exportBytes);

            var export = JsonConvert.DeserializeObject<ProjectExport>(exportJson);

            string projectJson = Encoding.UTF8.GetString(export.project);
            var parser = ProjectFactory.GetParser(projectJson);
            var project = parser.Parse(projectJson);

            string videosPath = Path.Combine(ProjectDirectory(name), VIDEOS);
            Directory.CreateDirectory(videosPath);

            //for (int i = 0; i < project.effects.Length; i++)
            //{
            //    if (project.effects[i] is Video video)
            //    {
            //        var video = project.effects[i];
            //        string videoName = Path.GetFileName(video.url);
            //        string path = Path.Combine(videosPath, videoName);
            //        File.WriteAllBytes(path, export.videos[i]);
            //    }
            //}

            project.version = ProjectFactory.VERSION;
            var projectPath = Path.Combine(ProjectDirectory(name), PROJECT_FILE);
            projectJson = JsonConvert.SerializeObject(project, Formatting.Indented);
            File.WriteAllText(projectPath, projectJson);

            return ProjectDirectory(name);
        }
        #endregion

        static string ProjectDirectory(string name)
        {
            string project = Path.Combine(ProjectsDirectory, name);
            if (!Directory.Exists(project))
                Directory.CreateDirectory(project);
            return project;
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
    public class ProjectExport
    {
        public byte[] project;
        public byte[][] videos;
    }
}
