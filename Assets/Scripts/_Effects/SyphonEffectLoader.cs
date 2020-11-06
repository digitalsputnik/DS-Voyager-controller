using System;
using System.Linq;
using Klak.Syphon;
using UnityEngine;

namespace VoyagerController.Effects
{
    public class SyphonEffectLoader : MonoBehaviour
    {
        public static SyphonCredentials[] AvailableServers { get; private set; }
        
        private void Awake()
        {
            if (Application.platform != RuntimePlatform.OSXPlayer &&
                Application.platform != RuntimePlatform.OSXEditor)
            {
                Destroy(this);
                return;
            }

            var effect = new SyphonEffect();
            
            EffectManager.AddEffect(effect);
            
            RefreshClients(() =>
            {
                if (!AvailableServers.Any()) return;
                effect.Server = AvailableServers[0];
                EffectManager.InvokeEffectModified(effect);
            });
        }

        public static void RefreshClients(Action refreshed)
        {
            var clients = SyphonHelper.GetListOfServers() ?? new (string, string)[0];
            AvailableServers = clients
                .Select(client => new SyphonCredentials(client.Item1, client.Item2))
                .ToArray();
            refreshed?.Invoke();
        }
    }
}