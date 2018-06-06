/* 
*   NatCam
*   Copyright (c) 2018 Yusuf Olokoba
*/

namespace NatCamU.Core.Platforms {

    using UnityEngine;

    public interface INatCam {

        #region --Events--
        event PreviewCallback OnStart;
        event PreviewCallback OnFrame;
        #endregion

        #region --Properties--
        INatCamDevice Device { get; }
        int Camera { get; set; }
        Texture Preview { get; }
        bool IsInitialized { get; }
        bool IsPlaying { get; }
        bool HasPermissions { get; }
        #endregion
        
        #region --Operations--
        void Play ();
        void Pause ();
        void Resume ();
        void Release ();
        void CapturePhoto (PhotoCallback callback);
        bool CaptureFrame (Texture2D frame);
        bool CaptureFrame (byte[] pixels, bool flip);
        #endregion
    }
}