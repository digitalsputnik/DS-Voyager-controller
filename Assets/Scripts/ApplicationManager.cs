﻿using System;
using DigitalSputnik;
using DigitalSputnik.Voyager;
using UnityEngine;

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
        #endregion
        
        #region Lamps
        
        private void Setup()
        {
            LampManager.Instance.OnLampDiscovered += LampDiscovered;
            LampManager.Instance.AddClient(new VoyagerClient());
            if (Application.isMobilePlatform && !Application.isEditor)
                LampManager.Instance.AddClient(new BluetoothClient());
        }

        private void Dispose()
        {
            Metadata.Clear();
            LampManager.Instance.RemoveClient<VoyagerClient>();
            if (Application.isMobilePlatform && !Application.isEditor)
                LampManager.Instance.RemoveClient<BluetoothClient>();
        }

        private void LampDiscovered(Lamp lamp)
        {
            if (AddLampToDatabase(lamp))
            {
                Debugger.LogInfo($"Lamp {lamp.Serial} discovered at {Metadata.Get(lamp.Serial).Discovered}");
                MainThread.Dispatch(() => OnLampDiscovered?.Invoke(lamp));
            }
        }

        private bool AddLampToDatabase(Lamp lamp)
        {
            try
            {
                if (lamp is VoyagerLamp voyager)
                {
                    Metadata.Add(voyager);
                    return true;   
                }
                else return false;
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