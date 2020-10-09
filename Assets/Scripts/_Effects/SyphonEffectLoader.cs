using System;
using System.Linq;
using System.Threading;
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
                return;
            
            var syphonEffect = new SyphonEffect();
            
            EffectManager.AddEffect(syphonEffect);
            
            RefreshClients(() =>
            {
                if (!AvailableServers.Any()) return;
                syphonEffect.Server = AvailableServers[0];
                EffectManager.InvokeEffectModified(syphonEffect);
            });
        }

        public static void RefreshClients(Action refreshed)
        {
            new Thread(() =>
            {
                var clients = SyphonHelper.GetListOfServers();
                AvailableServers = clients
                    .Select(client => new SyphonCredentials(client.Item1, client.Item2))
                    .ToArray();
                MainThread.Dispatch(() => refreshed?.Invoke());
            }).Start();
        }
    }
}