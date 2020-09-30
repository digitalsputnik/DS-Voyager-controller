using System;
using System.Collections.Generic;
using System.Linq;
using DigitalSputnik;

namespace VoyagerController
{
    public class LampDatabase
    {
        private const string ALREADY_CONTAINS_EXCEPTION = "Already contains lamp with the same serial";
        private const string UNKNOWN_LAMP_EXCEPTION = "No lamp with the serial found";
        
        private List<Lamp> _lamps;
        private Dictionary<string, LampMetadata> _metadata;

        public LampDatabase()
        {
            _lamps = new List<Lamp>();
            _metadata = new Dictionary<string, LampMetadata>();
        }

        public bool Contains(string serial)
        {
            return _lamps.Any(l => l.Serial == serial) && _metadata.ContainsKey(serial);
        } 

        public void Clear()
        {
            _lamps.Clear();
            _metadata.Clear();
        }

        public void Add<T>(Lamp lamp) where T : LampMetadata, new()
        {
            if (Contains(lamp.Serial))
                throw new Exception(ALREADY_CONTAINS_EXCEPTION);
            
            _lamps.Add(lamp);
            _metadata[lamp.Serial] = new T();
        }

        public IEnumerable<Lamp> GetLamps() => GetLamps<Lamp>();

        public IEnumerable<T> GetLamps<T>() where T : Lamp => _lamps.OfType<T>();

        public Lamp GetLamp(string serial) => GetLamp<Lamp>(serial);

        public T GetLamp<T>(string serial) where T : Lamp
        {
            if (!Contains(serial))
                throw new Exception(UNKNOWN_LAMP_EXCEPTION);

            return _lamps.FirstOrDefault(l => l.Serial == serial) as T;
        }

        public LampMetadata GetMetadata(string serial) => GetMetadata<LampMetadata>(serial);

        public T GetMetadata<T>(string serial) where T : LampMetadata
        {
            if (!Contains(serial))
                throw new Exception(UNKNOWN_LAMP_EXCEPTION);

            return _metadata[serial] as T;
        }
    }
    
    [Serializable]
    public class LampMetadata
    {
        public DateTime Discovered { get; set; }
    }
    
    [Serializable]
    public class VoyagerMetadata : LampMetadata { }
}