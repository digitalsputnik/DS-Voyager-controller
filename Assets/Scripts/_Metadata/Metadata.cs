using System;
using System.Collections.Generic;
using System.Linq;
using DigitalSputnik;
using DigitalSputnik.Voyager;
using UnityEngine;

namespace VoyagerController
{
    public class Metadata : MonoBehaviour
    {
        private static Metadata _instance;
        private void Awake() => _instance = this;

        private const string ALREADY_CONTAINS_EXCEPTION = "Already contains lamp with the same serial";
        private const string UNKNOWN_LAMP_EXCEPTION = "No lamp with the serial found";
        
        private Dictionary<string, LampMetadata> _metadata;

        public Metadata()
        {
            _metadata = new Dictionary<string, LampMetadata>();
        }

        public static bool Contains(string serial)
        {
            return _instance._metadata.ContainsKey(serial);
        }

        public static void Clear()
        {
            _instance._metadata.Clear();
        }

        public static void Add(VoyagerLamp lamp)
        {
            if (Contains(lamp.Serial))
                throw new Exception(ALREADY_CONTAINS_EXCEPTION);
            _instance._metadata[lamp.Serial] = new LampMetadata();
        }

        public static LampMetadata Get(string serial)
        {
            if (!Contains(serial))
                throw new Exception(UNKNOWN_LAMP_EXCEPTION);
            return _instance._metadata[serial];
        }

        public static IEnumerable<LampMetadata> Get(Func<LampMetadata, bool> predicate)
        {
            return _instance._metadata.Values.Where(predicate);
        }

        public static Dictionary<string, LampMetadata> GetAll() => _instance._metadata;

        public static void SetMetadata(string serial, LampMetadata metadata)
        {
            _instance._metadata[serial] = metadata;
        }
    }
}