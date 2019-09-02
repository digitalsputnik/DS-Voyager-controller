using System;
using UnityEngine;

namespace VoyagerApp.Videos
{
    [Serializable]
    public class Video
    {
        public string name;
        public string hash;
        public string path;
        public long frames;
        public float fps;
        public float duraction;
        public Texture2D thumbnail;
        public uint width;
        public uint height;
        public double lastStartTime;
        public double lastTimestamp;
    }
}