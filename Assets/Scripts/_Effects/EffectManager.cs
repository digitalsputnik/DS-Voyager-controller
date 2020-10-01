using System.Collections.Generic;
using System.IO;
using System.Linq;
using DigitalSputnik;
using DigitalSputnik.Voyager;
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
        
        public static string[] Presets => _instance._presets;
        
        public static void AddEffect(Effect effect)
        {
            if (_instance._effects.Any(p => p.Name == effect.Name)) return;
            
            _instance._effects.Add(effect);
            OnEffectAdded?.Invoke(effect);
            Debugger.LogInfo($"Effect {effect.Name} added");
        }

        public static void RemoveEffect(Effect effect)
        {
            if (!_instance._effects.Contains(effect) || IsEffectPreset(effect)) return;
            
            Destroy(effect.Meta.Thumbnail);
            _instance._effects.Remove(effect);
            OnEffectRemoved?.Invoke(effect);
        }

        internal static void InvokeEffectModified(Effect effect) => OnEffectModified?.Invoke(effect);

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
        #endregion
        
        [ContextMenu("Update Presets List")]
        private void UpdatePresetsList()
        {
            var path = Path.Combine(Application.streamingAssetsPath, "video_presets");
            _presets = Directory
                .GetFiles(path, "*.mp4")
                .Select(Path.GetFileNameWithoutExtension).
                ToArray();
        }
    }
}