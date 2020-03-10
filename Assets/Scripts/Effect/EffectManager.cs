using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VoyagerApp.Effects
{
    public class EffectManager : MonoBehaviour
    {
        #region Singleton
        internal static EffectManager instance;
        void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                LoadPresets();
            }
            else
                Destroy(gameObject);
        }
        #endregion

        public static Effect[] Effects => instance.effects.ToArray();
        public static EffectHandler onEffectAdded;
        public static EffectHandler onEffectRemoved;

        public event EffectHandler onEffectModified;

        [SerializeField] internal string[] videoPresets = null;

        List<Effect> effects = new List<Effect>();

        public static void AddEffect(Effect effect)
        {
            if (!instance.effects.Any(p => p.id == effect.id))
            {
                instance.effects.Add(effect);
                onEffectAdded?.Invoke(effect);
            }
        }

        public static void RemoveEffect(Effect effect)
        {
            if (Effects.Contains(effect) && !effect.preset)
            {
                Destroy(effect.thumbnail);
                instance.effects.Remove(effect);
                onEffectRemoved?.Invoke(effect);
            }
        }

        public static Effect GetEffectWithId(string id)
        {
            return Effects.FirstOrDefault(e => e.id == id);
        }

        public static T GetEffectWithId<T>(string id) where T : Effect
        {
            var effect = GetEffectWithId(id);
            if (effect is T t) return t;
            return null;
        }

        public static Effect GetEffectWithName(string name)
        {
            return Effects.FirstOrDefault(e => e.name == name);
        }

        public static T GetEffectWithName<T>(string name) where T : Effect
        {
            var effect = GetEffectWithName(name);
            if (effect is T t) return t;
            return null;
        }

        public static T[] GetEffectsOfType<T>() where T : Effect
        {
            List<T> effects = new List<T>();
            foreach (var effect in Effects)
                if (effect is T e) effects.Add(e);
            return effects.ToArray();
        }

        public static void Clear()
        {
            foreach (var effect in Effects)
            {
                if (!effect.preset)
                    instance.effects.Remove(effect);
            }
        }

        public void InvokeEffectChange(Effect effect)
        {
            onEffectModified?.Invoke(effect);
        }

        void LoadPresets()
        {
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
        }
    }

    public delegate void EffectHandler(Effect effect);
}
