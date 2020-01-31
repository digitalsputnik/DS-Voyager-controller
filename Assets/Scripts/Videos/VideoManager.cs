using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Video;
using VoyagerApp.Utilities;

namespace VoyagerApp.Videos
{
    public class VideoManager : MonoBehaviour
    {
        #region Singleton
        public static VideoManager instance;
        void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(this);
                LoadPresets();
            }
            else
                Destroy(this);
        }
        #endregion

        [SerializeField] float thumbnailTime = 0.5f;
        [SerializeField] float loadTimeout = 5.0f;
        [SerializeField] Vector2Int thumbnailSize;
        [Space(4)]
        [SerializeField] string[] effectPresets = null;

        public event VideoHandler onVideoAdded;
        public event VideoHandler onVideoRemoved;

        public List<Video> Videos { get; } = new List<Video>();

        public void Clear()
        {
            foreach (var video in new List<Video>(Videos))
                RemoveVideo(video);
        }

        public void LoadVideo(string path, VideoHandler onLoaded)
        {
            StartCoroutine(IEnumLoadVideo(path, "", 0, 0, onLoaded));
        }

        public void LoadVideo(string path, string guid, long frames, int fps, VideoHandler onLoaded)
        {
            StartCoroutine(IEnumLoadVideo(path, guid, frames, fps, onLoaded));
        }

        public void RemoveVideo(Video video)
        {
            if (Videos.Contains(video))
            {
                Destroy(video.thumbnail);
                Videos.Remove(video);
                onVideoRemoved?.Invoke(video);
            }
        }

        public Video GetWithName(string name)
        {
            return Videos.FirstOrDefault(v => v.name == name);
        }

        public Video GetWithHash(string hash)
        {
            return Videos.FirstOrDefault(v => v.hash == hash);
        }

        #region Video Loading
        public void LoadPresets()
        {
            if (Application.platform != RuntimePlatform.Android)
            {
                string prefabsPath = Path.Combine(Application.streamingAssetsPath, "Presets");
                foreach (var path in Directory.GetFiles(prefabsPath, "*.mp4"))
                    LoadVideo(path, null);
            }
            else
            {
                bool firstTime = !PlayerPrefs.HasKey("Opened");
                if (firstTime)
                {
                    StartCoroutine(IEnumSetupAndroidPresets());
                    PlayerPrefs.SetInt("Opened", 1);
                }
                else
                {
                    string prefabsPath = Path.Combine(Application.persistentDataPath, "Presets");
                    foreach (var path in Directory.GetFiles(prefabsPath, "*.mp4"))
                        LoadVideo(path, null);
                }
            }
        }

        IEnumerator IEnumSetupAndroidPresets()
        {
            string presetsDir = Path.Combine(Application.streamingAssetsPath, "Presets");
            string destDir = Path.Combine(Application.persistentDataPath, "Presets");

            Directory.CreateDirectory(destDir);

            foreach (var effectPreset in effectPresets)
            {
                string url = Path.Combine(presetsDir, effectPreset);
                string dest = Path.Combine(destDir, effectPreset);

                UnityWebRequest load = new UnityWebRequest(url);
                load.downloadHandler = new DownloadHandlerBuffer();
                yield return load.SendWebRequest();

                File.WriteAllBytes(dest, load.downloadHandler.data);
            }

            LoadPresets();
        }

        IEnumerator IEnumLoadVideo(string path, string guid, long frames, int fps, VideoHandler onLoaded)
        {
            Video video = new Video
            {
                hash = guid == "" ? Guid.NewGuid().ToString() : guid,
                fps = fps,
                frames = frames,
                lastTimestamp = 0.0,
                name = Path.GetFileNameWithoutExtension(path)
            };

            Videos.Add(video);

            var player = gameObject.AddComponent<VideoPlayer>();
            var render = SetupVideoPlayer(player, path);

            player.Play();

            float startTime = Time.time;

            yield return new WaitUntil(() => player.isPrepared ||
                                             LoadingTimeout(startTime));

            if (!LoadingTimeout(startTime))
            {
                for (ushort t = 0; t < player.audioTrackCount; t++)
                    player.SetDirectAudioMute(t, true);

                yield return new WaitForSeconds(thumbnailTime);

                VideoFromPlayer(ref video, player);
                onLoaded?.Invoke(video);
                onVideoAdded?.Invoke(video);
            }

            Destroy(player);
            Destroy(render);
        }

        RenderTexture SetupVideoPlayer(VideoPlayer player, string path)
        {
            var render = new RenderTexture(thumbnailSize.x,
                                           thumbnailSize.y,
                                           0,
                                           RenderTextureFormat.ARGB32);
            render.Create();

            player.url = path;
            player.renderMode = VideoRenderMode.RenderTexture;
            player.targetTexture = render;
            player.Prepare();

            return render;
        }

        void VideoFromPlayer(ref Video video, VideoPlayer player)
        {
            var url = player.url;
            var render = player.targetTexture;
            var thumbnail = TextureUtils.RenderTextureToTexture2D(render);

            video.path = url;
            video.frames = (long)player.frameCount;
            video.fps = video.fps == 0 ? (int)math.round(player.frameRate) : video.fps;
            video.duraction = player.frameCount / player.frameRate;
            video.thumbnail = thumbnail;
            video.width = player.width;
            video.height = player.height;
            video.lastStartTime = TimeUtils.Epoch;
        }

        bool LoadingTimeout(float startTime)
        {
            return Time.time - startTime > loadTimeout;
        }
        #endregion
    }

    public delegate void VideoHandler(Video video);
}