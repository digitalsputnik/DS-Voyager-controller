using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
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

        public List<Video> Videos { get; } = new List<Video>();

        public void Clear()
        {
            foreach (var video in new List<Video>(Videos))
                RemoveVideo(video);
        }

        public void AddLoadedVideo(Video video)
        {
            Videos.Add(video);
            onVideoAdded?.Invoke(video);
        }

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
                onVideoRemoved?.Invoke(video);
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
                height = player.height,
                lastStartTime = TimeUtils.Epoch
            };
        }

        bool LoadingTimeout(float startTime)
        {
            return Time.time - startTime > loadTimeout;
        }
        #endregion
    }

    public delegate void VideoHandler(Video video);
}