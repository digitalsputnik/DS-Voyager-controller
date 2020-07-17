using UnityEngine;

namespace DigitalSputnik.Videos.iOS
{
    public delegate void ErrorHandler(string error);
    
    public delegate void VideoResizedHandler(string path);
    
    public class IosVideoResizerListener : MonoBehaviour
    {
        public event ErrorHandler OnError;
        public event VideoResizedHandler OnResized;

        // Called from objective-c
        public void ResizeError(string error)
        {
            OnError?.Invoke(error);
        }

        // Called from objective-c
        public void ResizeDone(string path)
        {
            OnResized?.Invoke(path);
        }
    }
}