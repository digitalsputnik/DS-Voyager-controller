using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using VoyagerApp.Lamps;
using VoyagerApp.Networking;
using VoyagerApp.Utilities;
using VoyagerApp.Videos;
using VoyagerApp.Workspace;

namespace VoyagerApp.Projects
{
    [Serializable]
    public class Project
    {
        public const string PROJECT_FILE = "project.dsprj";
        public const string VIDEOS = "videos";

        public List<Video> videos;
        public List<Lamp> lamps;
        public float camPositionX;
        public float camPositionY;
        public float camSize;
        public List<WorkspaceItemSaveData> items;

        Project WithVideos(List<Video> videos)
        {
            this.videos = videos;
            return this;
        }

        Project WithLamps(List<Lamp> lamps)
        {
            this.lamps = lamps;
            return this;
        }

        Project WithItems(List<WorkspaceItemSaveData> items)
        {
            this.items = items;
            return this;
        }

        Project WithCamera(Camera camera)
        {
            camSize = camera.orthographicSize;
            camPositionX = camera.transform.position.x;
            camPositionY = camera.transform.position.y;
            return this;
        }

        #region Save
        public static void Save(string name)
        {
            var videos = VideoManager.instance.Videos;
            var lamps = LampManager.instance.Lamps;
            var items = GetItemsSaveDatas();
            var camera = Camera.main;

            CopyVideosIfNecessary(name, videos);

            var project = new Project()
                .WithVideos(videos)
                .WithLamps(lamps)
                .WithItems(items)
                .WithCamera(camera);

            var settings = JsonSettings();
            string json = JsonConvert.SerializeObject(project, settings);

            var path = Path.Combine(ProjectDirectory(name), PROJECT_FILE);
            File.WriteAllText(path, json);
        }

        static List<WorkspaceItemSaveData> GetItemsSaveDatas()
        {
            int itemsCount = WorkspaceManager.instance.Items.Count;
            var items = new List<WorkspaceItemSaveData>();
            for (int i = 0; i < itemsCount; i++)
            {
                var item = WorkspaceManager.instance.Items[i];
                items.Add(item.ToData());
            }
            return items;
        }
        #endregion

        #region Loading
        public static void Load(string name)
        {
            var path = Path.Combine(ProjectDirectory(name), PROJECT_FILE);

            if (!File.Exists(path))
            {
                Debug.LogError($"The project {name} could not be found!");
                return;
            }

            string json = File.ReadAllText(path);
            var settings = JsonSettings();
            var project = JsonConvert.DeserializeObject<Project>(json, settings);

            LoadVideos(project);
            LoadLamps(project);
            LoadItems(project);
            LoadCamera(project);
        }

        static void LoadVideos(Project project)
        {
            VideoManager.instance.Clear();
            foreach (var video in project.videos)
                VideoManager.instance.AddLoadedVideo(video);
        }

        static void LoadLamps(Project project)
        {
            LampManager.instance.Clear();
            foreach (var lamp in project.lamps)
                LampManager.instance.AddLamp(lamp);
        }

        static void LoadItems(Project project)
        {
            var manager = WorkspaceManager.instance;
            manager.Clear();
            foreach (var item in project.items)
                item.Load();
            foreach (var item in project.items)
            {
                if (item.parentguid != "")
                {
                    var child = manager.GetItem(item.guid);
                    var parent = manager.GetItem(item.parentguid);
                    child.SetParent(parent);
                }
            }
        }

        static void LoadCamera(Project project)
        {
            var cam = Camera.main;
            float camPositionZ = cam.transform.position.z;
            var camPosition = new Vector3(
                project.camPositionX,
                project.camPositionY,
                camPositionZ);

            cam.orthographicSize = project.camSize;
            cam.transform.position = camPosition;
        }
        #endregion

        #region Import / Export
        public static string Export(string save)
        {
            Save(save);

            var projectPath = Path.Combine(ProjectDirectory(save), PROJECT_FILE);
            byte[] projectBytes = File.ReadAllBytes(projectPath);

            string json = File.ReadAllText(projectPath);
            var settings = JsonSettings();
            var project = JsonConvert.DeserializeObject<Project>(json, settings);

            byte[][] videosBytes = new byte[project.videos.Count][];
            for (int i = 0; i < project.videos.Count; i++)
                videosBytes[i] = File.ReadAllBytes(project.videos[i].path);

            ProjectExport export = new ProjectExport
            {
                project = projectBytes,
                videos = videosBytes
            };

            json = JsonConvert.SerializeObject(export);
            byte[] bytes = Encoding.UTF8.GetBytes(json);

            string exportPath = Path.Combine(Application.temporaryCachePath, save + "vcp");
            File.WriteAllBytes(exportPath, bytes);

            Directory.Delete(ProjectDirectory(save), true);
            return exportPath;
        }
        public static string Import(string file)
        {
            string name = Path.GetFileNameWithoutExtension(file);

            byte[] exportBytes = File.ReadAllBytes(file);
            string exportJson = Encoding.UTF8.GetString(exportBytes);

            var export = JsonConvert.DeserializeObject<ProjectExport>(exportJson);

            string projectJson = Encoding.UTF8.GetString(export.project);
            var settings = JsonSettings();
            var project = JsonConvert.DeserializeObject<Project>(projectJson, settings);

            var videosPath = Path.Combine(ProjectDirectory(name), VIDEOS);
            Directory.CreateDirectory(videosPath);

            for (int i = 0; i < project.videos.Count; i++)
            {
                string videoName = Path.GetFileName(project.videos[i].path);
                string newPath = Path.Combine(videosPath, videoName);
                File.WriteAllBytes(newPath, export.videos[i]);
                project.videos[i].path = newPath;
            }

            var projectPath = Path.Combine(ProjectDirectory(name), PROJECT_FILE);
            projectJson = JsonConvert.SerializeObject(project, settings);
            File.WriteAllText(projectPath, projectJson);

            return ProjectDirectory(name);
        }
        #endregion

        public static JsonSerializerSettings JsonSettings()
        {
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new IPAddressConverter());
            settings.Converters.Add(new IPEndPointConverter());
            settings.Converters.Add(new Texture2DConverter());
            settings.Formatting = Formatting.Indented;
            settings.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
            settings.TypeNameHandling = TypeNameHandling.Auto;
            return settings;
        }

        public static string ProjectsDirectory
        {
            get
            {
                string persistant = Application.persistentDataPath;
                string projects = Path.Combine(persistant, "projects");

                if (!Directory.Exists(projects))
                    Directory.CreateDirectory(projects);

                return projects;
            }
        }

        static string ProjectDirectory(string name)
        {
            string project = Path.Combine(ProjectsDirectory, name);
            if (!Directory.Exists(project))
                Directory.CreateDirectory(project);
            return project;
        }

        static void CopyVideosIfNecessary(string name, List<Video> videos)
        {
            var path = Path.Combine(ProjectDirectory(name), VIDEOS);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            foreach (var video in videos)
            {
                if (!video.path.StartsWith(path, StringComparison.Ordinal))
                {
                    string videoName = Path.GetFileName(video.path);
                    string newPath = Path.Combine(path, videoName);

                    if (!File.Exists(newPath))
                        File.Copy(video.path, newPath);

                    video.path = newPath;
                }
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
