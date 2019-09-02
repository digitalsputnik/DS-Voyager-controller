﻿using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;
using VoyagerApp.Lamps.Voyager;

namespace VoyagerApp.Lamps
{
    public class LampManager : MonoBehaviour
    {
        #region Singleton
        public static LampManager instance;
        void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(this);
            }
            else
                Destroy(this);
        }
        #endregion

        public List<Lamp> Lamps = new List<Lamp>();
        public List<Lamp> ConnectedLamps => GetConnectedLamps();

        public event LampHandler onLampAdded;
        public event LampHandler onLampRemoved;

        List<LampDataProcessor> dataProcessors = new List<LampDataProcessor>();

        void Start()
        {
            AddDataProcessors();
        }

        public void AddLamp(Lamp lamp)
        {
            if (!Lamps.Any(_ => _.serial == lamp.serial))
            {
                Lamps.Add(lamp);
                onLampAdded?.Invoke(lamp);
            }
        }

        public void RemoveLamp(Lamp lamp)
        {
            if (Lamps.Any(_ => _.serial == lamp.serial))
            {
                Lamp same = instance.GetLampWithSerial(lamp.serial);
                Lamps.Remove(same);
                onLampRemoved?.Invoke(same);
            }
        }

        public void Clear() => new List<Lamp>(Lamps).ForEach(RemoveLamp);

        public Lamp GetLampWithAddress(IPAddress address)
        {
            string add = address.ToString();
            return Lamps.FirstOrDefault(_ => _.address.ToString() == add);
        }

        public Lamp GetLampWithSerial(string serial)
        {
            return Lamps.FirstOrDefault(_ => _.serial == serial);
        }

        void AddDataProcessors()
        {
            dataProcessors.Add(new VoyagerDataProcessor(this));
        }

        List<Lamp> GetConnectedLamps()
        {
            return Lamps.Where(_ => _.connected).ToList();
        }
    }

    public delegate void LampHandler(Lamp lamp);

    internal abstract class LampDataProcessor
    {
        protected readonly LampManager manager;

        internal LampDataProcessor(LampManager manager)
        {
            this.manager = manager;
        }
    }
}