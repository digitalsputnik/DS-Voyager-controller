using System;
using System.Collections.Generic;
using DigitalSputnik;

namespace VoyagerController
{
    public class LampDatabase
    {
        private Dictionary<string, LampMetadata> _metadata;

        public LampDatabase()
        {
            _metadata = new Dictionary<string, LampMetadata>();
        }

        public bool Contains(string serial) => _metadata.ContainsKey(serial);

        public void Clear() => _metadata.Clear();
        
        public void Create<T>(string serial) where T : LampMetadata, new() => _metadata[serial] = new T();

        public LampMetadata Get(Lamp lamp) => Get<LampMetadata>(lamp);

        public T Get<T>(Lamp lamp) where T : LampMetadata, new() => Get<T>(lamp.Serial);

        public LampMetadata Get(string serial) => Get<LampMetadata>(serial);

        public T Get<T>(string serial) where T : LampMetadata, new()
        {
            if (!Contains(serial))
            {
                Debugger.LogWarning("Tried to get metadata of unknown lamp. Data instance created.");
                Create<T>(serial);
            }
            
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