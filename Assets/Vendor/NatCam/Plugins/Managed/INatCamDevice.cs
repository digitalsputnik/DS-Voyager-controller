/* 
*   NatCam
*   Copyright (c) 2018 Yusuf Olokoba
*/

namespace NatCamU.Core.Platforms {

    public interface INatCamDevice {
        
        #region --Properties--
        bool IsFrontFacing (int camera);
        bool IsFlashSupported (int camera);
        bool IsTorchSupported (int camera);
        float HorizontalFOV (int camera);
        float VerticalFOV (int camera);
        float MinExposureBias (int camera);
        float MaxExposureBias (int camera);
        float MaxZoomRatio (int camera);
        #endregion

        #region --Getters--
        CameraResolution GetPreviewResolution (int camera);
        CameraResolution GetPhotoResolution (int camera);
        float GetFramerate (int camera);
        float GetExposure (int camera);
        int GetExposureMode (int camera);
        int GetFocusMode (int camera);
        int GetFlash (int camera);
        bool GetTorchEnabled (int camera);
        float GetZoom (int camera);
        #endregion

        #region --Setters--
        void SetPreviewResolution (int camera, CameraResolution resolution);
        void SetPhotoResolution (int camera, CameraResolution resolution);
        void SetFramerate (int camera, float framerate);
        void SetFocus (int camera, float x, float y);
        void SetExposure (int camera, float bias);
        void SetExposureMode (int camera, int state);
        void SetFocusMode (int camera, int state);
        void SetFlash (int camera, int state);
        void SetTorchEnabled (int camera, bool enabled);
        void SetZoom (int camera, float ratio);
        #endregion
    }
}