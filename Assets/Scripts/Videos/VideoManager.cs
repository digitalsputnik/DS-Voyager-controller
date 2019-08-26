using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Video;
using VoyagerApp.Networking;
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

                jsonSettings = new JsonSerializerSettings();
                jsonSettings.Converters.Add(new Texture2DConverter());
                jsonSettings.Formatting = Formatting.Indented;

                LoadVideos();
            }
            else
                Destroy(this);
        }
        #endregion

        [SerializeField] float thumbnailTime = 0.5f;
        [SerializeField] float loadTimeout = 5.0f;
        [SerializeField] Vector2Int thumbnailSize;

        public event VideoHandler onVideoAdded;
        public event VideoHandler onVideoRemoved;

        JsonSerializerSettings jsonSettings;

        public List<Video> Videos { get; } = new List<Video>();

        public void LoadVideo(string path, VideoHandler onLoaded)
        {
            StartCoroutine(IEnumLoadVideo(path, onLoaded));
        }

        public void RemoveVideo(Video video)
        {
            if (Videos.Contains(video))
            {
                Destroy(video.thumbnail);
                Videos.Remove(video);
                onVideoRemoved(video);
                SaveVideos();
            }
        }

        #region Video Loading
        IEnumerator IEnumLoadVideo(string path, VideoHandler onLoaded)
        {
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

                Video video = VideoFromPlayer(player);

                Videos.Add(video);
                onLoaded?.Invoke(video);
                onVideoAdded?.Invoke(video);
                SaveVideos();
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

        Video VideoFromPlayer(VideoPlayer player)
        {
            var url = player.url;
            var render = player.targetTexture;
            var thumbnail = TextureUtils.RenderTextureToTexture2D(render);

            return new Video
            {
                name = Path.GetFileNameWithoutExtension(url),
                hash = Guid.NewGuid().ToString(),
                path = url,
                frames = (long)player.frameCount,
                fps = player.frameRate,
                duraction = player.frameCount / player.frameRate,
                thumbnail = thumbnail,
                width = player.width,
                height = player.height
            };
        }

        bool LoadingTimeout(float startTime)
        {
            return Time.time - startTime > loadTimeout;
        }
        #endregion

        #region Save & Load
        void SaveVideos()
        {
            string json = JsonConvert.SerializeObject(Videos, jsonSettings);
            File.WriteAllText(MetadataPath, json);
        }

        void LoadVideos()
        {
            if (!File.Exists(MetadataPath)) return;
            string json = File.ReadAllText(MetadataPath);
            var videos = JsonConvert.DeserializeObject<List<Video>>(json, jsonSettings);

            foreach (var video in videos)
            {
                if (!Videos.Any(_ => _.hash == video.hash))
                {
                    Videos.Add(video);
                    onVideoAdded?.Invoke(video);
                }
            }
        }

        static string MetadataPath
        {
            get => Path.Combine(Application.persistentDataPath, "videos_meta.vmd");
        }
        #endregion
    }

    public delegate void VideoHandler(Video video);
}