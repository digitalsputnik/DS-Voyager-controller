using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Video;

namespace DigitalSputnik.Videos
{
    public class UnityVideoProvider : IVideoProvider
    {
        private const int THUMBNAIL_WIDTH = 160;
        private const int THUMBNAIL_HEIGHT = 90;
        private const float VIDEO_THUMBNAIL_TIME = 1.0f;
        private const float VIDEO_LOAD_TIMEOUT = 10.0f;

        static Queue<VideoLoaderQueueItem> loadVideoQueue = new Queue<VideoLoaderQueueItem>();
        static bool queueHandlerRunning = false;

        public void LoadVideo(string path, VideoHandler loaded)
        {
            if (Application.platform == RuntimePlatform.Android)
            { 
                //Older android devices can crash if videos are loaded too quickly, that is why we load then from a queue.
                loadVideoQueue.Enqueue(new VideoLoaderQueueItem(path, loaded));

                if (!queueHandlerRunning)
                {
                    var obj = LoaderObject();
                    obj.StartCoroutine(LoadVideoQueueHandler());
                }
            }
            else
            {
                var obj = LoaderObject();
                var player = obj.gameObject.AddComponent<VideoPlayer>();
                obj.StartCoroutine(LoadVideoIEnumerator(player, path, loaded));
            }
        }

        private IEnumerator LoadVideoQueueHandler()
        {
            queueHandlerRunning = true;

            while (loadVideoQueue.Count > 0)
            {
                var video = loadVideoQueue.Dequeue();

                var obj = LoaderObject();
                var player = obj.gameObject.AddComponent<VideoPlayer>();
                obj.StartCoroutine(LoadVideoIEnumerator(player, video.Path, video.Handler));

                yield return new WaitForSeconds(0.2f);
            }

            queueHandlerRunning = false;
        }

        public bool Rename(ref Video video, string name)
        {
            if (!IsNameCorrect(name)) return false;
            
            var directory = Path.GetDirectoryName(video.Path) ?? "";
            var extension = Path.GetExtension(video.Path);
            var path = Path.Combine(directory, $"{name}.{extension}");
            
            try
            {
                File.Move(video.Path ?? "", path);
                video.Path = path;
                video.Name = name;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return false;
            }
        }

        public bool IsNameCorrect(string name) => Regex.IsMatch(name, @"^[\w\-. ]+$");

        private static UnityVideoLoaderBehaviour LoaderObject()
        {
            return new GameObject("Video Loader").AddComponent<UnityVideoLoaderBehaviour>();
        }
        
        private IEnumerator LoadVideoIEnumerator(VideoPlayer player, string path, VideoHandler loaded)
        {
            if (!File.Exists(path)) loaded?.Invoke(null, null);

            var render = CreateThumbnailRenderer();
            player.url = path;
            player.renderMode = VideoRenderMode.RenderTexture;
            player.targetTexture = render;
            player.Prepare();
            
            var startTime = Time.time;

            yield return new WaitUntil(() => player.isPrepared || LoadingTimeout(startTime));

            if (!LoadingTimeout(startTime))
            {
                for (ushort t = 0; t < player.audioTrackCount; t++)
                    player.SetDirectAudioMute(t, true);

                player.Play();

                yield return new WaitForSeconds(VIDEO_THUMBNAIL_TIME);

                var thumbnail = ToTexture2D(render);

                var video = new Video
                {
                    Width = (int) player.width,
                    Height = (int) player.height,
                    Name = Path.GetFileNameWithoutExtension(path),
                    Path = path,
                    FrameCount = player.frameCount,
                    Fps = player.frameRate
                };
                
                loaded?.Invoke(video, thumbnail);
            }
            else
                loaded?.Invoke(null, null);

            UnityEngine.Object.Destroy(player.gameObject);
            UnityEngine.Object.Destroy(render);
        }

        public Texture2D ToTexture2D(RenderTexture rTex)
        {
            Texture2D dest = new Texture2D(rTex.width, rTex.height, TextureFormat.RGBA32, false);
            dest.Apply(false);
            Graphics.CopyTexture(rTex, dest);
            return dest;
        }

        private static RenderTexture CreateThumbnailRenderer()
        {
            var render = new RenderTexture(
                THUMBNAIL_WIDTH,
                THUMBNAIL_HEIGHT,
                0,
                RenderTextureFormat.ARGB32);
            render.Create();
            return render;
        }

        private static bool LoadingTimeout(float startTime)
        {
            return Time.time - startTime > VIDEO_LOAD_TIMEOUT;
        }
    }
    
    public class UnityVideoLoaderBehaviour : MonoBehaviour { }

    internal class VideoLoaderQueueItem
    {
        public string Path;
        public VideoHandler Handler;
        public VideoLoaderQueueItem(string path, VideoHandler handler) { Path = path; Handler = handler; }
    }
}