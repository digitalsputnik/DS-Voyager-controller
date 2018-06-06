/* 
*   NatCam
*   Copyright (c) 2018 Yusuf Olokoba
*/

using UnityEngine;

namespace NatCamU.Core.Platforms {

    public class NatCamDeviceAndroid : INatCamDevice {

        #region --Properties--
        public bool IsFrontFacing (int camera) {
            return this[camera].Call<bool>("isFrontFacing");
        }

        public bool IsFlashSupported (int camera) {
            return this[camera].Call<bool>("isFlashSupported");
        }

        public bool IsTorchSupported (int camera) {
            return this[camera].Call<bool>("isTorchSupported");
        }

        public float HorizontalFOV (int camera) {
            return this[camera].Call<float>("horizontalFOV");
        }

        public float VerticalFOV (int camera) {
            return this[camera].Call<float>("verticalFOV");
        }

        public float MinExposureBias (int camera) {
            return this[camera].Call<float>("minExposureBias");
        }

        public float MaxExposureBias (int camera) {
            return this[camera].Call<float>("maxExposureBias");
        }

        public float MaxZoomRatio (int camera) {
            return this[camera].Call<float>("maxZoomRatio");
        }
        #endregion


        #region --Getters--

        public CameraResolution GetPreviewResolution (int camera) {
            AndroidJavaObject jRet = this[camera].Call<AndroidJavaObject>("getPreviewResolution");
            if (jRet.GetRawObject().ToInt32() == 0) return new CameraResolution(0, 0);
            int[] res = AndroidJNIHelper.ConvertFromJNIArray<int[]>(jRet.GetRawObject());
            return new CameraResolution(res[0], res[1]);
        }

        public CameraResolution GetPhotoResolution (int camera) {
            AndroidJavaObject jRet = this[camera].Call<AndroidJavaObject>("getPhotoResolution");
            if (jRet.GetRawObject().ToInt32() == 0) return new CameraResolution(0, 0);
            int[] res = AndroidJNIHelper.ConvertFromJNIArray<int[]>(jRet.GetRawObject());
            return new CameraResolution(res[0], res[1]);
        }

        public float GetFramerate (int camera) {
            return this[camera].Call<float>("getFramerate");
        }
        
        public float GetExposure (int camera) {
            return this[camera].Call<float>("getExposure");
        }

        public int GetExposureMode (int camera) {
            return this[camera].Call<int>("getExposureMode");
        }

        public int GetFocusMode (int camera) {
            return this[camera].Call<int>("getFocusMode");
        }

        public int GetFlash (int camera) {
            return this[camera].Call<int>("getFlash");
        }

        public bool GetTorchEnabled (int camera) {
            return this[camera].Call<bool>("getTorchEnabled");
        }
        
        public float GetZoom (int camera) {
            return this[camera].Call<float>("getZoom");
        }
        #endregion


        #region --Setters--
        public void SetPreviewResolution (int camera, CameraResolution resolution) {
            this[camera].Call("setResolution", resolution.width, resolution.height);
        }

        public void SetPhotoResolution (int camera, CameraResolution resolution) {
            this[camera].Call("setPhotoResolution", resolution.width, resolution.height);
        }

        public void SetFramerate (int camera, float framerate) {
            this[camera].Call("setFramerate", framerate);
        }

        public void SetFocus (int camera, float x, float y) {
            this[camera].Call("setFocus", x, y);
        }

        public void SetExposure (int camera, float bias) {
            this[camera].Call("setExposure", (int)bias);
        }

        public void SetExposureMode (int camera, int state) {
            this[camera].Call("setExposureMode", state);
        }

        public void SetFocusMode (int camera, int state) {
            this[camera].Call("setFocusMode", state);
        }

        public void SetFlash (int camera, int state) {
            this[camera].Call("setFlash", state);
        }

        public void SetTorchEnabled (int camera, bool enabled) {
            this[camera].Call("setTorchEnabled", enabled);
        }
        public void SetZoom (int camera, float ratio) {
            this[camera].Call("setZoom", ratio);
        }
        #endregion
        

        #region --Interop--

        private readonly AndroidJavaClass natcamdevice;

        public NatCamDeviceAndroid () {
            natcamdevice = new AndroidJavaClass("com.yusufolokoba.natcam.NatCamDevice");
        }

        public AndroidJavaObject this [int index] {
            get {
                return natcamdevice.CallStatic<AndroidJavaObject>("getCamera", index);
            }
        }
        #endregion
    }
}