using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DigitalSputnik;
using DigitalSputnik.Videos;
using UnityEngine;
using UnityEngine.Networking;
#if UNITY_IOS && !UNITY_EDITOR
using DigitalSputnik.Videos.iOS;
#endif

namespace VoyagerController.Effects
{
    [RequireComponent(typeof(ThumbnailLoader))]
    public class VideoEffectLoader : MonoBehaviour
    {
        private static VideoEffectLoader _instance;

        private ThumbnailLoader _thumbnailLoader;

        private void Awake()
        {
            _instance = this;
            _thumbnailLoader = GetComponent<ThumbnailLoader>();
            LoadPresets();
        }

        private static VideoTools _tools;
        
        private static VideoTools Tools
        {
            get
            {
                if (_tools == null) InitializeVideoTools();
                return _tools;
            }
        }
        
        private static void InitializeVideoTools()
        {
            var resizer = CreateVideoResizerBasedOnPlatform();
            _tools = new VideoTools(new UnityVideoProvider(), resizer);
        }
        
        private static IVideoResizer CreateVideoResizerBasedOnPlatform()
        {
            #if UNITY_IOS && !UNITY_EDITOR
            return new IosVideoResizer();
            #elif UNITY_ANDROID && !UNITY_EDITOR
            return new AndroidVideoResizer();
            #else
            return new NotImplementedVideoResizer();
            #endif
        }
        
        public static void LoadVideoEffect(string path, EffectHandler loaded)
        {
            Tools.LoadVideo(path, (video, thumbnail) =>
            {
                var effect = new VideoEffect(video)
                {
                    Meta =
                    {
                        StartTime = TimeUtils.Epoch,
                        Thumbnail = thumbnail
                    }
                };
                
                EffectManager.AddEffect(effect);
            });
        }
        
        public static void ResizeVideo(VideoEffect video, int width, int height, EffectHandler done)
        {
            Tools.Resize(video.Video, width, height, (success, error) =>
            {
                MainThread.Dispatch(() =>
                {
                    if (!success)
                    {
                        Debugger.LogError(error);
                        done?.Invoke(null);
                    }

                    EffectManager.InvokeEffectModified(video);
                    done?.Invoke(video);
                });
            });
        }
        
        #region Presets
        private static void LoadPresets()
        {
            if (Application.platform != RuntimePlatform.Android)
            {
                var path = Path.Combine(Application.streamingAssetsPath, "video_presets");
                LoadPresetsFrom(path);
            }
            else
            {
                var newPresets = GetNewPresets();

                if (newPresets.Count > 0)
                {
                    _instance.StartCoroutine(
                        EnumSetupAndroidPresets(newPresets.ToArray()));
                }
                else
                {
                    string path = Path.Combine(Application.persistentDataPath, "video_presets");
                    LoadPresetsFrom(path);
                }
            }
        }

        static List<string> GetNewPresets()
        {
            var path = Path.Combine(Application.persistentDataPath, "video_presets");
            var allPresets = EffectManager.Presets;

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            var loadedPresetPaths = Directory.GetFiles(path, "*.mp4");

            List<string> loadedPresetFileNames = new List<string>();
            List<string> newPresets = new List<string>();

            foreach (var presetPath in loadedPresetPaths)
                loadedPresetFileNames.Add(Path.GetFileName(presetPath));

            foreach (var preset in allPresets)
            {
                if (!loadedPresetFileNames.Contains(preset + ".mp4"))
                    newPresets.Add(preset);
            }

            return newPresets;
        }


        private static IEnumerator EnumSetupAndroidPresets(IEnumerable<string> presets)
        {
            var source = Path.Combine(Application.streamingAssetsPath, "video_presets");
            var destination = Path.Combine(Application.persistentDataPath, "video_presets");

            Directory.CreateDirectory(destination);

            foreach (var preset in presets)
            {
                var url = Path.Combine(source, preset) + ".mp4";
                var dest = Path.Combine(destination, preset) + ".mp4";
                
                var load = new UnityWebRequest(url) { downloadHandler = new DownloadHandlerBuffer() };

                yield return load.SendWebRequest();

                if (load.result == UnityWebRequest.Result.ConnectionError) Debug.Log(load.error);
                
                File.WriteAllBytes(dest, load.downloadHandler.data);
            }

            LoadPresets();
        }

        private static void LoadPresetsFrom(string path)
        {
            foreach (var p in Directory.GetFiles(path, "*.mp4"))
                LoadVideoEffect(p, effect => effect.Id = Guid.NewGuid().ToString());
        }
        #endregion
    }
}