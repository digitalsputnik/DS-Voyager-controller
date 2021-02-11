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
        private const float VIDEO_LOAD_TIMEOUT = 10.0f;

        static Queue<VideoLoaderQueueItem> loadVideoQueue = new Queue<VideoLoaderQueueItem>();
        static bool queueHandlerRunning = false;

        public void LoadVideo(string path, VideoHandler loaded)
        {
            //Older android devices can crash if videos are loaded too quickly, that is why we load then from a queue.
            loadVideoQueue.Enqueue(new VideoLoaderQueueItem(path, loaded));
            
            if (!queueHandlerRunning)
            {
                var obj = LoaderObject();
                obj.StartCoroutine(LoadVideoQueueHandler());
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

                yield return new WaitForSeconds(0.5f);
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
            if (!File.Exists(path)) loaded?.Invoke(null);

            player.url = path;
            player.renderMode = VideoRenderMode.APIOnly;
            player.Prepare();
            
            var startTime = Time.time;

            yield return new WaitUntil(() => player.isPrepared || LoadingTimeout(startTime));

            if (!LoadingTimeout(startTime))
            {
                for (ushort t = 0; t < player.audioTrackCount; t++)
                    player.SetDirectAudioMute(t, true);

                yield return new WaitForSeconds(1.0f);

                var video = new Video
                {
                    Width = (int) player.width,
                    Height = (int) player.height,
                    Name = Path.GetFileNameWithoutExtension(path),
                    Path = path,
                    FrameCount = player.frameCount,
                    Fps = player.frameRate
                };
                
                loaded?.Invoke(video);
            }
            else
                loaded?.Invoke(null);

            UnityEngine.Object.Destroy(player.gameObject);
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