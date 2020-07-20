using DigitalSputnik.Videos;
using UnityEngine;

namespace VoyagerApp
{
    public class NotImplementedVideoResizer : IVideoResizer
    {
        public bool Resize(ref Video video, int width, int height)
        {
            Debug.LogError("Using \"UnityNotImplementedVideoResizer\" to resize video.");
            return false;
        }
    }
}