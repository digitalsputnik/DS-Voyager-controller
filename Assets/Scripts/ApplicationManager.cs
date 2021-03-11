using System;
using DigitalSputnik;
using DigitalSputnik.Networking;
using DigitalSputnik.Voyager;
using UnityEngine;
using UnityEngine.Networking;
using VoyagerController.Bluetooth;

namespace VoyagerController
{
    public class ApplicationManager : MonoBehaviour
    {
        #region Instance
        public static ApplicationManager Instance = null;
        
        public void Awake()
        {
            if (Instance == null)
            {
                Setup();
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (this == Instance)
            {
                Dispose();
            }
        }

        #endregion
        
        #region Static
        public static LampHandler OnLampDiscovered;

        public static LampHandler OnLampBroadcasted;
        #endregion

        #region Lamps

        private void Setup()
        {
            NetworkTransport.SetMulticastLock(true);

            NetUtils.UseInterfaceForAddress = !Application.isMobilePlatform;

            LampManager.Instance.OnLampDiscovered += LampDiscovered;
            LampManager.Instance.OnLampBroadcasted += LampBroadcasted;
            
            LampManager.Instance.AddClient(new VoyagerNetworkClient());
            
            if (Application.isMobilePlatform && !Application.isEditor) 
                LampManager.Instance.AddClient(new VoyagerBluetoothClient());
        }
        
        private void Dispose()
        {
            Metadata.Clear();
            LampManager.Instance.RemoveClient<VoyagerNetworkClient>();
            
            if (Application.isMobilePlatform && !Application.isEditor) 
                LampManager.Instance.RemoveClient<VoyagerBluetoothClient>();
        }

        private void LampDiscovered(Lamp lamp)
        {
            if (AddLampToDatabase(lamp))
            {
                Debugger.LogInfo($"Lamp {lamp.Serial} discovered at {Metadata.Get<LampData>(lamp.Serial).Discovered}");
                MainThread.Dispatch(() => OnLampDiscovered?.Invoke(lamp));
            }
        }

        private void LampBroadcasted(Lamp lamp)
        {
            Debugger.LogInfo($"Lamp {lamp.Serial} broadcasted");
            MainThread.Dispatch(() => OnLampBroadcasted?.Invoke(lamp));
        }

        private bool AddLampToDatabase(Lamp lamp)
        {
            try
            {
                if (lamp is VoyagerLamp voyager)
                {
                    Metadata.Add<LampData>(voyager.Serial);
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                Debugger.LogError(ex.Message);
                return false;
            }
        }
        #endregion
    }

    public delegate void LampHandler(Lamp lamp);
}