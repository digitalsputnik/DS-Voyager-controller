using System;
using System.Collections.Generic;
using System.Linq;
using DigitalSputnik.Colors;
using DigitalSputnik.Voyager;
using VoyagerController.Effects;

namespace VoyagerController
{
    public class LampDatabase
    {
        private const string ALREADY_CONTAINS_EXCEPTION = "Already contains lamp with the same serial";
        private const string UNKNOWN_LAMP_EXCEPTION = "No lamp with the serial found";
        
        private List<VoyagerLamp> _lamps;
        private Dictionary<string, LampMetadata> _metadata;

        public LampDatabase()
        {
            _lamps = new List<VoyagerLamp>();
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

        public void Add(VoyagerLamp lamp)
        {
            if (Contains(lamp.Serial))
                throw new Exception(ALREADY_CONTAINS_EXCEPTION);
            
            _lamps.Add(lamp);
            _metadata[lamp.Serial] = new LampMetadata();
        }

        public IEnumerable<VoyagerLamp> GetLamps() => _lamps;

        public VoyagerLamp GetLamp(string serial)
        {
            if (!Contains(serial))
                throw new Exception(UNKNOWN_LAMP_EXCEPTION);
            return _lamps.FirstOrDefault(l => l.Serial == serial);
        }

        public LampMetadata GetMetadata(string serial)
        {
            if (!Contains(serial))
                throw new Exception(UNKNOWN_LAMP_EXCEPTION);

            return _metadata[serial];
        }

        public IEnumerable<LampMetadata> GetMetadata(Func<LampMetadata, bool> predicate)
        {
            return _metadata.Values.Where(predicate);
        }
    }
    
    [Serializable]
    public class LampMetadata
    {
        public DateTime Discovered { get; set; }
        public Effect Effect { get; set; }
        public double TimeEffectApplied { get; set; }
        public bool Rendered { get; set; } = false;
        public Rgb[][] FrameBuffer { get; set; }
    }
}