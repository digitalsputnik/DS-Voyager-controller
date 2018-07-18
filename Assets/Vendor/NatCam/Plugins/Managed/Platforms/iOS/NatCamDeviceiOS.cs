/* 
*   NatCam
*   Copyright (c) 2018 Yusuf Olokoba
*/

namespace NatCamU.Core.Platforms {

    public class NatCamDeviceiOS : INatCamDevice {

        #region --Properties--
        public bool IsFrontFacing (int camera) {
            return camera.IsFrontFacing();
        }

        public bool IsFlashSupported (int camera) {
            return camera.IsFlashSupported();
        }

        public bool IsTorchSupported (int camera) {
            return camera.IsTorchSupported();
        }

        public float HorizontalFOV (int camera) {
            return camera.HorizontalFOV();
        }

        public float VerticalFOV (int camera) {
            return camera.VerticalFOV();
        }

        public float MinExposureBias (int camera) {
            return camera.MinExposureBias();
        }

        public float MaxExposureBias (int camera) {
            return camera.MaxExposureBias();
        }

        public float MaxZoomRatio (int camera) {
            return camera.MaxZoomRatio();
        }
        #endregion


        #region --Getters--
        public CameraResolution GetPreviewResolution (int camera) {
            int width, height;
            camera.GetPreviewResolution(out width, out height);
            return new CameraResolution(width, height);
        }

        public CameraResolution GetPhotoResolution (int camera) {
            int width, height;
            camera.GetPhotoResolution(out width, out height);
            return new CameraResolution(width, height);
        }

        public float GetFramerate (int camera) {
            return camera.GetFramerate();
        }
        
        public float GetExposure (int camera) {
            return camera.GetExposure();
        }
        public int GetExposureMode (int camera) {
            return camera.GetExposureMode();
        }
        public int GetFocusMode (int camera) {
            return camera.GetFocusMode();
        }
        public int GetFlash (int camera) {
            return camera.GetFlash();
        }
        public bool GetTorchEnabled (int camera) {
            return camera.GetTorchEnabled();
        }
        public float GetZoom (int camera) {
            return camera.GetZoom();
        }
        #endregion


        #region --Setters--
        
        public void SetPreviewResolution (int camera, CameraResolution resolution) {
            camera.SetPreviewResolution(resolution.width, resolution.height);
        }

        public void SetPhotoResolution (int camera, CameraResolution resolution) {
            camera.SetPhotoResolution(resolution.width, resolution.height);
        }

        public void SetFramerate (int camera, float framerate) {
            camera.SetFramerate(framerate);
        }

        public void SetFocus (int camera, float x, float y) {
            camera.SetFocus(x, y);
        }

        public void SetExposure (int camera, float bias) {
            camera.SetExposure(bias);
        }

        public void SetExposureMode (int camera, int state) {
            camera.SetExposureMode(state);
        }

        public void SetFocusMode (int camera, int state) {
            camera.SetFocusMode(state);
        }

        public void SetFlash (int camera, int state) {
            camera.SetFlash(state);
        }

        public void SetTorchEnabled (int camera, bool enabled) {
            camera.SetTorchEnabled(enabled);
        }
        public void SetZoom (int camera, float ratio) {
            camera.SetZoom(ratio);
        }
        #endregion
    }
}