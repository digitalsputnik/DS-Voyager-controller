using System;
using System.Linq;
using System.Threading;
using Klak.Spout;
using UnityEngine;

namespace VoyagerController.Effects
{
    public class SpoutEffectLoader : MonoBehaviour
    {
        public static string[] AvailableSources { get; private set; }
        
        private void Awake()
        {
            if (Application.platform != RuntimePlatform.WindowsPlayer &&
                Application.platform != RuntimePlatform.WindowsEditor)
            {
                Destroy(this);
                return;
            }

            var effect = new SpoutEffect();
            
            EffectManager.AddEffect(effect);

            RefreshSources(() =>
            {
                if (!AvailableSources.Any()) return;
                effect.Source = AvailableSources[0];
                EffectManager.InvokeEffectModified(effect);
            });
        }

        public static void RefreshSources(Action refreshed)
        {
            new Thread(() =>
            {
                var sources = SpoutManager.GetSourceNames();
                AvailableSources = sources;
                MainThread.Dispatch(refreshed);
            }).Start();
        }
    }
}