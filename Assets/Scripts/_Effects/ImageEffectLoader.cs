using DigitalSputnik;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using VoyagerController.Effects;

namespace VoyagerApp
{
    public class ImageEffectLoader : MonoBehaviour
    {
        private static ImageEffectLoader _instance;

        private void Awake()
        {
            _instance = this;
            LoadPresets();
        }

        public static void LoadImageEffect(string path, EffectHandler loaded)
        {
            var image = new ImageEffect(path) { Meta = { Timestamp = TimeUtils.Epoch } };
            EffectManager.AddEffect(image);
        }

        private static void LoadPresets()
        {
            if (Application.platform != RuntimePlatform.Android)
            {
                var path = Path.Combine(Application.streamingAssetsPath, "image_presets");
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
                    string path = Path.Combine(Application.persistentDataPath, "image_presets");
                    LoadPresetsFrom(path);
                }
            }
        }

        static List<string> GetNewPresets()
        {
            var path = Path.Combine(Application.persistentDataPath, "image_presets");
            var allPresets = EffectManager.ImagePresets;

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            var loadedPresetPaths = Directory.GetFiles(path, "*.png");

            List<string> loadedPresetFileNames = new List<string>();
            List<string> newPresets = new List<string>();

            foreach (var presetPath in loadedPresetPaths)
                loadedPresetFileNames.Add(Path.GetFileName(presetPath));

            foreach (var preset in allPresets)
            {
                if (!loadedPresetFileNames.Contains(preset + ".png"))
                    newPresets.Add(preset);
            }

            return newPresets;
        }


        private static IEnumerator EnumSetupAndroidPresets(IEnumerable<string> presets)
        {
            var source = Path.Combine(Application.streamingAssetsPath, "image_presets");
            var destination = Path.Combine(Application.persistentDataPath, "image_presets");

            Directory.CreateDirectory(destination);

            foreach (var preset in presets)
            {
                var url = Path.Combine(source, preset) + ".png";
                var dest = Path.Combine(destination, preset) + ".png";

                var load = new UnityWebRequest(url) { downloadHandler = new DownloadHandlerBuffer() };

                yield return load.SendWebRequest();

                if (load.result == UnityWebRequest.Result.ConnectionError) Debug.Log(load.error);

                File.WriteAllBytes(dest, load.downloadHandler.data);
            }

            LoadPresets();
        }

        private static void LoadPresetsFrom(string path)
        {
            foreach (var p in Directory.GetFiles(path, "*.png"))
                LoadImageEffect(p, effect => effect.Id = Guid.NewGuid().ToString());
        }
    }
}
