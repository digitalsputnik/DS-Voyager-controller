using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VoyagerController
{
    public class Metadata : MonoBehaviour
    {
        private static Metadata _instance;
        private void Awake() => _instance = this;
        
        private const string ALREADY_CONTAINS_EXCEPTION = "Already contains data with the key.";
        private const string UNKNOWN_KEY_EXCEPTION = "No data with the key found.";
        private const string WRONG_TYPE_EXCEPTION = "Data with the key found has different type.";
        
        private readonly Dictionary<string, Data> _metadata = new Dictionary<string, Data>();
        
        public static bool Contains(string key)
        {
            return !string.IsNullOrEmpty(key) && _instance._metadata.ContainsKey(key);
        }

        public static void Clear()
        {
            _instance._metadata.Clear();
        }

        public static void Add<T>(string key) where T : Data
        {
            if (Contains(key))
                throw new Exception(ALREADY_CONTAINS_EXCEPTION);
            _instance._metadata.Add(key, (T)Activator.CreateInstance(typeof(T)));
        }

        public static void Remove(string key)
        {
            if (!Contains(key))
                throw new Exception(UNKNOWN_KEY_EXCEPTION);
            _instance._metadata.Remove(key);
        }

        public static T Get<T>(string key) where T : Data
        {
            if (!Contains(key))
                throw new Exception(UNKNOWN_KEY_EXCEPTION);

            if (_instance._metadata[key] is T data) return data;

            throw new Exception(WRONG_TYPE_EXCEPTION);
        }

        public static void Set<T>(string key, T value) where T : Data
        {
            if (!Contains(key))
                throw new Exception(UNKNOWN_KEY_EXCEPTION);
            _instance._metadata[key] = value;
        }

        public static IEnumerable<T> Get<T>() where T : Data
        {
            return _instance._metadata.Values.OfType<T>();
        }

        public static Dictionary<string, T> GetPairs<T>() where T : Data
        {
            var pairs = new Dictionary<string, T>();

            foreach (var pair in _instance._metadata)
            {
                if (pair.Value is T value)
                    pairs.Add(pair.Key, value);
            }

            return pairs;
        }
    }
}