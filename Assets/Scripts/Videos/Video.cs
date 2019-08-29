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

    public struct VideoPosition
    {
        public float x1;
        public float y1;
        public float x2;
        public float y2;

        public VideoPosition(float x1, float y1, float x2, float y2)
        {
            this.x1 = x1;
            this.y1 = y1;
            this.x2 = x2;
            this.y2 = y2;
        }

        public VideoPosition(Vector2 start, Vector2 end)
        {
            x1 = start.x;
            y1 = start.y;
            x2 = end.x;
            y2 = end.x;
        }
    }
}