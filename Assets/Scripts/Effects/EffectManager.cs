using System.Collections.Generic;
using System.Linq;
using DigitalSputnik.Videos;
using UnityEngine;

#if UNITY_IOS && !UNITY_EDITOR
using DigitalSputnik.Videos.iOS;
#endif

namespace VoyagerController.Effects
{
    public class EffectManager : MonoBehaviour
    {
        #region Singleton
        private static EffectManager _instance;
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                LoadPresets();
            }
            else
                Destroy(gameObject);
        }
        #endregion
        
        #region Effects
        public static EffectHandler OnEffectAdded;
        public static EffectHandler OnEffectRemoved;
        public static EffectHandler OnEffectModified;

        [SerializeField] private string[] _presets = null;

        private readonly List<Effect> _effects = new List<Effect>();
        
        public static void AddEffect(Effect effect)
        {
            if (_instance._effects.Any(p => p.Id == effect.Id)) return;
            
            _instance._effects.Add(effect);
            OnEffectAdded?.Invoke(effect);
        }

        public static void RemoveEffect(Effect effect)
        {
            if (!_instance._effects.Contains(effect) || IsEffectPreset(effect)) return;
            
            Destroy(effect.Meta.Thumbnail);
            _instance._effects.Remove(effect);
            OnEffectRemoved?.Invoke(effect);
        }

        public static IEnumerable<T> GetEffects<T>() where T : Effect => _instance._effects.OfType<T>();

        public static IEnumerable<Effect> GetEffects() => GetEffects<Effect>();

        public static Effect GetEffectWithId(string id)
        {
            return GetEffects().FirstOrDefault(e => e.Id == id);
        }

        public static T GetEffectWithId<T>(string id) where T : Effect
        {
            return GetEffects<T>().FirstOrDefault(e => e.Id == id);
        }

        public static Effect GetEffectWithName(string name)
        {
            return GetEffects().FirstOrDefault(e => e.Name == name);
        }

        public static T GetEffectWithName<T>(string name) where T : Effect
        {
            return GetEffects<T>().FirstOrDefault(e => e.Name == name);
        }

        public static bool IsEffectPreset(Effect effect)
        {
            return _instance._presets.Any(e => e != effect.Name);
        }
        
        public static void Clear()
        {
            foreach (var effect in _instance._effects.ToList().Where(effect => !IsEffectPreset(effect)))
                _instance._effects.Remove(effect);
        }
        
        private static void LoadPresets()
        {
            /*
            VideoEffectLoader.LoadVideoPresets();

            if (Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor)
            {
                AddEffect(new SyphonStream
                {
                    preset = true,
                    name = "Syphon Stream",
                    id = System.Guid.NewGuid().ToString(),
                    available = new EventValue<bool>(true)
                });
            }

            if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
            {
                AddEffect(new SpoutStream
                {
                    preset = true,
                    name = "Spout Stream",
                    id = System.Guid.NewGuid().ToString(),
                    available = new EventValue<bool>(true)
                });
            }
            */
        }
        #endregion
        
        #region Video
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
            _tools = new DigitalSputnik.Videos.VideoTools(new UnityVideoProvider(), resizer);
        }
        
        private static IVideoResizer CreateVideoResizerBasedOnPlatform()
        {
            #if UNITY_IOS && !UNITY_EDITOR
            return new IosVideoResizer();
            #else
            return new NotImplementedVideoResizer();
            #endif
        }
        
        public static void LoadVideo(string path, EffectHandler loaded)
        {
            Tools.LoadVideo(path, video => loaded?.Invoke(new VideoEffect(video)));
        }

        public static void ResizeVideo(VideoEffect video, int width, int height, EffectHandler done)
        {
            Tools.Resize(video.Video, width, height, (success, error) =>
            {
                if (!success) Debugger.LogError(error);
                done?.Invoke(video);
            });
        }
        #endregion
    }
}