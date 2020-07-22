using DigitalSputnik.Videos;
using UnityEngine;

namespace VoyagerApp
{
    public class NotImplementedVideoResizer : IVideoResizer
    {
        public void Resize(Video video, int width, int height, VideoResizeHandler resized)
        {
            resized?.Invoke(false, "Using \"UnityNotImplementedVideoResizer\" to resize video.");
        }
    }
}