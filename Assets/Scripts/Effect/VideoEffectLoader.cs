using System;
using System.Collections;
using System.IO;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Video;
using VoyagerApp.Utilities;

namespace VoyagerApp.Effects
{
    public static class VideoEffectLoader
    {
        const int VIDEO_THUMBNAIL_WIDTH = 160;
        const int VIDEO_THUMBNAIL_HEIGHT = 90;
        const float VIDEO_THUMBNAIL_TIMEOUT = 10.0f;
        const float VIDEO_THUMBNAIL_TIME = 2.0f;

        public static Video LoadVideo(Video video)
        {
            EffectManager.instance.StartCoroutine(
                LoadVideo(EffectManager.instance.gameObject, video));
            EffectManager.AddEffect(video);
            return video;
        }

        public static Video LoadNewVideoFromPath(string path)
        {
            Video video = new Video
            {
                id = Guid.NewGuid().ToString(),
                file = Path.GetFileName(path),
                name = Path.GetFileNameWithoutExtension(path),
                path = path,
                preset = EffectManager.instance.videoPresets.Any(p => p == Path.GetFileName(path))
            };

            EffectManager.instance.StartCoroutine(
                LoadVideo(EffectManager.instance.gameObject, video));
            EffectManager.AddEffect(video);
            return video;
        }

        internal static IEnumerator LoadVideo(GameObject obj, Video video)
        {
            var player = obj.AddComponent<VideoPlayer>();
            var render = SetupVideoPlayer(player, video.path);

            player.Play();

            float startTime = (float)TimeUtils.Epoch;
            yield return new WaitUntil(() => player.isPrepared || LoadingTimeout(startTime));

            if (!LoadingTimeout(startTime))
            {
                for (ushort t = 0; t < player.audioTrackCount; t++)
                    player.SetDirectAudioMute(t, true);

                yield return new WaitForSeconds(VIDEO_THUMBNAIL_TIME);

                SetupVideoFromPlayer(ref video, player);
            }

            UnityEngine.Object.Destroy(player);
            UnityEngine.Object.Destroy(render);
        }

        static RenderTexture SetupVideoPlayer(VideoPlayer player, string path)
        {
            var render = new RenderTexture(VIDEO_THUMBNAIL_WIDTH,
                                           VIDEO_THUMBNAIL_HEIGHT,
                                           0,
                                           RenderTextureFormat.ARGB32);
            render.Create();

            player.url = path;
            player.renderMode = VideoRenderMode.RenderTexture;
            player.targetTexture = render;
            player.Prepare();

            return render;
        }

        static bool LoadingTimeout(float startTime)
        {
            return Time.time - startTime > VIDEO_THUMBNAIL_TIMEOUT;
        }

        static void SetupVideoFromPlayer(ref Video video, VideoPlayer player)
        {
            var render = player.targetTexture;
            var thumbnail = TextureUtils.RenderTextureToTexture2D(render);

            video.frames = (long)player.frameCount;
            video.fps = video.fps == 0 ? (int)math.round(player.frameRate) : video.fps;
            video.thumbnail = thumbnail;
            video.width = player.width;
            video.height = player.height;
            video.startTime = TimeUtils.Epoch;
            video.available.value = true;
        }

        internal static void LoadVideoPresets()
        {
            if (Application.platform != RuntimePlatform.Android)
            {
                string path = Path.Combine(Application.streamingAssetsPath, "video_presets");
                LoadPresetsFrom(path);
            }
            else
            {
                if (!PlayerPrefs.HasKey("prefabs_loaded"))
                {
                    PlayerPrefs.SetInt("prefabs_loaded", 1);
                    EffectManager.instance.StartCoroutine(
                        IEnumSetupAndroidPresets(EffectManager.instance.videoPresets));
                }
                else
                {
                    string path = Path.Combine(Application.persistentDataPath, "video_presets");
                    LoadPresetsFrom(path);
                }
            }
        }

        static IEnumerator IEnumSetupAndroidPresets(string[] presets)
        {
            string source = Path.Combine(Application.streamingAssetsPath, "video_presets");
            string destin = Path.Combine(Application.persistentDataPath, "video_presets");

            Directory.CreateDirectory(destin);

            foreach (var preset in presets)
            {
                string url = Path.Combine(source, preset);
                string dest = Path.Combine(destin, preset);

                UnityWebRequest load = new UnityWebRequest(url);
                load.downloadHandler = new DownloadHandlerBuffer();

                yield return load.SendWebRequest();

                if (load.isNetworkError)
                    Debug.Log(load.error);

                File.WriteAllBytes(dest, load.downloadHandler.data);
            }

            LoadVideoPresets();
        }

        static void LoadPresetsFrom(string path)
        {
            foreach (var p in Directory.GetFiles(path, "*.mp4"))
                LoadNewVideoFromPath(p);
        }
    }
}
