using System;
using UnityEngine;

namespace VoyagerApp.Videos
{
    [Serializable]
    public class VideoPosition
    {
        public float x1;
        public float y1;
        public float x2;
        public float y2;

        public VideoPosition()
        {
            x1 = -0.5f;
            y1 =  0.0f;
            x2 =  0.5f;
            y2 =  0.0f;
        }

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
            y2 = end.y;
        }
    }
}