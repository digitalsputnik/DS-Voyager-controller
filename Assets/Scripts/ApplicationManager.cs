using System;
using System.Collections.Generic;
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

        #region Events
        public static LampHandler OnLampDiscovered;
        #endregion
        
        private readonly LampDatabase _database = new LampDatabase();
        private readonly List<Lamp> _lamps = new List<Lamp>();
        
        private void Setup()
        {
            LampManager.Instance.OnLampDiscovered += LampDiscovered;
            LampManager.Instance.AddClient(new VoyagerClient());
        }

        private void Dispose()
        {
            LampManager.Instance.RemoveClient<VoyagerClient>();
            _database.Clear();
            _lamps.Clear();
        }

        private void LampDiscovered(Lamp lamp)
        {
            _lamps.Add(lamp);
            CreateMetadataBasedOnType(lamp);

            _database.Get(lamp).Discovered = DateTime.UtcNow;
            
            Debugger.LogInfo($"Lamp {lamp.Serial} discovered at {_database.Get(lamp.Serial).Discovered}");
        }

        private void CreateMetadataBasedOnType(Lamp lamp)
        {
            var serial = lamp.Serial;
            
            switch (lamp)
            {
                case VoyagerLamp _:
                    _database.Create<VoyagerMetadata>(serial);
                    break;
                default:
                    _database.Create<LampMetadata>(serial);
                    break;
            }
        }
    }

    public delegate void LampHandler(Lamp lamp);
}