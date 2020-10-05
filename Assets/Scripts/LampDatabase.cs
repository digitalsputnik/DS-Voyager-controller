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
        public EffectMapping EffectMapping { get; set; } = new EffectMapping();
        public double TimeEffectApplied { get; set; }
        public bool Rendered { get; set; } = false;
        public Rgb[][] FrameBuffer { get; set; }
    }

    public class EffectMapping
    {
        public float[] Positions { get; set; } = { 0.0f, 0.5f, 1.0f, 0.5f };

        public float X1
        {
            get => Positions[0];
            set => Positions[0] = value;
        }
        
        public float Y1
        {
            get => Positions[1];
            set => Positions[1] = value;
        }
        
        public float X2
        {
            get => Positions[2];
            set => Positions[2] = value;
        }
        
        public float Y2
        {
            get => Positions[3];
            set => Positions[3] = value;
        }
    }
}