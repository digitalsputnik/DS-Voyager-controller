using DigitalSputnik.Videos;

#if UNITY_IOS && !UNITY_EDITOR
using DigitalSputnik.Videos.iOS;
#endif

namespace VoyagerApp
{
    /// <summary>
    /// This class is kind of a global singleton. Every class that's needed only as one instance
    /// can be hooked up to this. Through this class you can get access to common tools and handlers.
    /// </summary>
    public static class App
    {
        #region Videos
        /// <summary>
        /// Video tools provides everything to load, rename, remove and resize a video.
        /// </summary>
        public static VideoTools VideoTools
        {
            get
            {
                if (_videoTools != null) return _videoTools;
                var resizer = CreateVideoResizerBasedOnPlatform();
                return _videoTools = new VideoTools(new UnityVideoProvider(), resizer);
            }
        }

        private static VideoTools _videoTools;

        private static IVideoResizer CreateVideoResizerBasedOnPlatform()
        {
            #if UNITY_IOS && !UNITY_EDITOR
            return new IosVideoResizer();
            #else
            return new NotImplementedVideoResizer();
            #endif
        }
        #endregion
    }
}