using System;
using System.Collections.Generic;
using System.Linq;
using DigitalSputnik.Voyager;
using UnityEngine;

namespace VoyagerController
{
    public class Metadata : MonoBehaviour
    {
        private static Metadata _instance;
        private void Awake() => _instance = this;

        private const string ALREADY_CONTAINS_LAMP_EXCEPTION = "Already contains lamp with the same serial";
        private const string UNKNOWN_LAMP_EXCEPTION = "No lamp with the serial found";
        
        private const string ALREADY_CONTAINS_PICTURE_EXCEPTION = "Already contains picture with the same serial";
        private const string UNKNOWN_PICTURE_EXCEPTION = "No picture with the serial found";
        
        private readonly Dictionary<string, LampMetadata> _lampMetadata;
        private readonly Dictionary<string, PictureMetadata> _pictureMetadata;

        public Metadata()
        {
            _lampMetadata = new Dictionary<string, LampMetadata>();
            _pictureMetadata = new Dictionary<string, PictureMetadata>();
        }
        
        public static void Clear()
        {
            _instance._lampMetadata.Clear();
        }
        
        #region Lamps
        public static bool ContainsLamp(string serial)
        {
            return !string.IsNullOrEmpty(serial) &&  _instance._lampMetadata.ContainsKey(serial);
        }

        public static void AddLamp(VoyagerLamp lamp)
        {
            if (ContainsLamp(lamp.Serial))
                throw new Exception(ALREADY_CONTAINS_LAMP_EXCEPTION);
            _instance._lampMetadata[lamp.Serial] = new LampMetadata();
        }

        public static LampMetadata GetLamp(string serial)
        {
            if (!ContainsLamp(serial))
                throw new Exception(UNKNOWN_LAMP_EXCEPTION);
            return _instance._lampMetadata[serial];
        }

        public static IEnumerable<LampMetadata> GetLamp(Func<LampMetadata, bool> predicate)
        {
            return _instance._lampMetadata.Values.Where(predicate);
        }

        public static Dictionary<string, LampMetadata> GetAllLamps() => _instance._lampMetadata;

        public static void SetLampMetadata(string serial, LampMetadata metadata)
        {
            _instance._lampMetadata[serial] = metadata;
        }
        #endregion
        
        #region Picture
        public static bool ContainsPicture(string id)
        {
            return !string.IsNullOrEmpty(id) && _instance._pictureMetadata.ContainsKey(id);
        }

        public static void AddPicture(string id)
        {
            if (ContainsPicture(id))
                throw new Exception(ALREADY_CONTAINS_PICTURE_EXCEPTION);
            _instance._pictureMetadata[id] = new PictureMetadata();
        }

        public static void RemovePicture(string id)
        {
            if (ContainsPicture(id))
                _instance._pictureMetadata.Remove(id);
        }

        public static PictureMetadata GetPicture(string id)
        {
            if (!ContainsPicture(id))
                throw new Exception(UNKNOWN_PICTURE_EXCEPTION);
            return _instance._pictureMetadata[id];
        }

        public static Dictionary<string, PictureMetadata> GetAllPictures() => _instance._pictureMetadata;

        public static void SetPictureMetadata(string serial, PictureMetadata metadata)
        {
            _instance._pictureMetadata[serial] = metadata;
        }
        #endregion
    }
}