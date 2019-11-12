using System;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Unity.Mathematics;
using UnityEngine;
using VoyagerApp.Lamps;
using VoyagerApp.Lamps.Voyager;
using VoyagerApp.Videos;
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

        public static ProjectSaveData Save(string name) => Save(name, false);
        public static ProjectSaveData Save(string name, bool keepVideoPaths)
        {
            var paths = CopyVideosIfNecessary(name);
            var projectData = ProjectFactory.GetCurrentSaveData();
            var json = JsonConvert.SerializeObject(projectData, Formatting.Indented);
            var path = Path.Combine(ProjectDirectory(name), PROJECT_FILE);
            File.WriteAllText(path, json);

            if (keepVideoPaths)
            {
                for (int i = 0; i < VideoManager.instance.Videos.Count; i++)
                    VideoManager.instance.Videos[i].path = paths[i];
            }

            return projectData;
        }

        public static void SaveWorkspace()
        {
            var data = ProjectFactory.GetCurrentWorkspaceData();
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            PlayerPrefs.SetString("workspace_temp", json);
        }

        static string[] CopyVideosIfNecessary(string name)
        {
            var path = Path.Combine(ProjectDirectory(name), VIDEOS);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            string[] paths = new string[VideoManager.instance.Videos.Count];
            int i = 0;

            foreach (var video in VideoManager.instance.Videos)
            {
                paths[i++] = video.path;

                if (!video.path.StartsWith(path, StringComparison.Ordinal))
                {
                    string videoName = Path.GetFileName(video.path);
                    string newPath = Path.Combine(path, videoName);

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
        public static void Load(string name)
        {
            string path = Path.Combine(ProjectsDirectory, name, PROJECT_FILE);
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                var parser = ProjectFactory.GetParser(json);
                var data = parser.Parse(json);
                Load(data);
            }
        }

        public static void LoadWorkspace()
        {
            if (PlayerPrefs.HasKey("workspace_temp"))
            {
                var json = PlayerPrefs.GetString("workspace_temp");
                var data = WorkspaceDataParser.Parser(json);
                LoadItems(data.items);
                LoadCamera(data.camera);
            }
        }

        public static ProjectSaveData GetProjectData(string json)
        {
            var parser = ProjectFactory.GetParser(json);
            return parser.Parse(json);
        }

        static void Load(ProjectSaveData data)
        {
            LoadVideos(data.videos);
            LoadLamps(data.lamps);
            LoadItems(data.items);
            LoadCamera(data.camera);
        }

        static void LoadVideos(Video[] videos)
        {
            VideoManager.instance.Clear();
            foreach (var videoData in videos)
            {
                if (File.Exists(videoData.url))
                {
                    VideoManager.instance.LoadVideo(
                        videoData.url,
                        videoData.guid,
                        videoData.frames,
                        videoData.fps,
                        null);
                }
            }
        }

        static void LoadLamps(Lamp[] lamps)
        {
            //LampManager.instance.Clear();
            foreach (var lampData in lamps)
            {
                var buffer = new VideoBuffer();

                if (lampData.buffer != null)
                {
                    buffer.RecreateBuffer(lampData.buffer.Length);
                    buffer.framesToBuffer = lampData.buffer;
                }

                var itsh = new Itshe(
                    lampData.itsh[0],
                    lampData.itsh[1],
                    lampData.itsh[2],
                    lampData.itsh[3],
                    lampData.itsh[4]
                );

                var mapping = new VideoPosition();

                if (lampData.mapping != null)
                {
                    mapping = new VideoPosition(
                        lampData.mapping[0],
                        lampData.mapping[1],
                        lampData.mapping[2],
                        lampData.mapping[3]
                    );
                }

                var lamp = LampManager.instance.GetLampWithSerial(lampData.serial);

                if (lamp == null)
                {
                    lamp = new VoyagerLamp
                    {
                        serial = lampData.serial,
                        length = lampData.length,
                        video = VideoManager.instance.GetWithHash(lampData.video),
                        address = IPAddress.Parse(lampData.address),
                        itshe = itsh,
                        mapping = mapping,
                        buffer = buffer
                    };

                    LampManager.instance.AddLamp(lamp);
                }
                else
                {
                    lamp.video = VideoManager.instance.GetWithHash(lampData.video);
                    lamp.itshe = itsh;
                    lamp.mapping = mapping;
                    lamp.buffer = buffer;
                }
            }
        }

        static void LoadItems(Item[] items)
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

        static void LoadCamera(float[] camera)
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

            try
            {
                var data = Save(EXPORT_TEMP, true);

                byte[] project = File.ReadAllBytes(projPath);
                byte[][] videos = new byte[data.videos.Length][];
                for (int i = 0; i < videos.Length; i++)
                    videos[i] = File.ReadAllBytes(data.videos[i].url);

                ProjectExport export = new ProjectExport
                {
                    project = project,
                    videos = videos
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

            for (int i = 0; i < project.videos.Length; i++)
            {
                var video = project.videos[i];
                string videoName = Path.GetFileName(video.url);
                string path = Path.Combine(videosPath, videoName);
                File.WriteAllBytes(path, export.videos[i]);
                project.videos[i].url = path;
            }

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
                string persistant = Application.persistentDataPath;
                string projects = Path.Combine(persistant, PROJECTS_DIRECTORY);

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
