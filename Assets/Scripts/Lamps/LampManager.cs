using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;
using VoyagerApp.Effects;
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

        public List<Lamp> UpdatedLamps = new List<Lamp>();

        public event LampHandler onLampOutdated;
        public event LampHandler onLampAdded;
        public event LampHandler onLampRemoved;

        public event LampHandler onLampMappingChanged;
        public event LampHandler onLampEffectChanged;
        public event LampHandler onLampItsheChanged;
        public event LampHandler onLampBroadcasted;

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

        public void LampBroadcasted(Lamp lamp) => onLampBroadcasted?.Invoke(lamp);

        public void Clear() => new List<Lamp>(Lamps).ForEach(RemoveLamp);

        public Lamp GetLampWithAddress(IPAddress address)
        {
            string add = address.ToString();
            return Lamps.FirstOrDefault(_ => _.address?.ToString() == add);
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
        public List<Lamp> LampsWithEffect(Effect effect)
        {
            return Lamps.Where(lamp => lamp.effect != null && lamp.effect.id == effect.id).ToList();
        }

        internal void RaiseLampMappingChangedEvent(Lamp lamp)
        {
            onLampMappingChanged?.Invoke(lamp);
        }

        internal void RaiseLampEffectChangedEvent(Lamp lamp)
        {
            onLampEffectChanged?.Invoke(lamp);
        }

        internal void RaiseLampItsheChangedEvent(Lamp lamp)
        {
            onLampItsheChanged?.Invoke(lamp);
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