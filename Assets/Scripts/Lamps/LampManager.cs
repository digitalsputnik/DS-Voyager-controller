using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;
using VoyagerApp.Lamps.Voyager;
using VoyagerApp.Videos;

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

        public List<Lamp> UpdatedLamps = new List<Lamp>();

        public event LampHandler onLampOutdated;
        public event LampHandler onLampAdded;
        public event LampHandler onLampRemoved;

        List<LampDataProcessor> dataProcessors = new List<LampDataProcessor>();

        void Start()
        {
            AddDataProcessors();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.I))
            {
                foreach(var lamp in Lamps)
                    Debug.Log($"{lamp.serial} - {lamp.address}");
            }
        }

        public void AddLamp(Lamp lamp)
        {
            if (!Lamps.Any(_ => _.serial == lamp.serial))
            {
                Lamps.Add(lamp);
                onLampAdded?.Invoke(lamp);

                if (!lamp.updated)
                    onLampOutdated?.Invoke(lamp);
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

        public List<Lamp> GetConnectedLamps()
        {
            return Lamps.Where(_ => _.connected).ToList();
        }

        // TODO: Move to utilities.
        public List<Lamp> LampsWithVideo(Video video)
        {
            return Lamps.Where(lamp => lamp.video.hash == video.hash).ToList();
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