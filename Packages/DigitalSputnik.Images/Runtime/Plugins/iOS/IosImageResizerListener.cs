#if UNITY_IOS
using UnityEngine;

namespace DigitalSputnik.Images.iOS
{
    public delegate void ImageResizedHandler(string path);
    
    public class IosImageResizerListener : MonoBehaviour
    {
        public event ImageResizedHandler OnResized;

        // Called from objective-c
        public void ResizeDone(string path)
        {
            OnResized?.Invoke(path);
        }
    }
}
#endif