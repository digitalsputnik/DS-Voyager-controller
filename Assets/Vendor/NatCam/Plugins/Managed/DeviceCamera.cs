/* 
*   NatCam
*   Copyright (c) 2018 Yusuf Olokoba
*/

namespace NatCamU.Core {

    using UnityEngine;
    using System.Linq;
    using Platforms;
    using Docs;

    [Doc(@"DeviceCamera")]
    public sealed class DeviceCamera {

        #region --Getters--

        /// <summary>
        /// Is the camera front facing?
        /// </summary>
        [Doc(@"IsFrontFacing")]
        public bool IsFrontFacing { get { return Device.IsFrontFacing(this); }}
        /// <summary>
        /// Does this camera support flash?
        /// </summary>
        [Doc(@"IsFlashSupported")]
        public bool IsFlashSupported { get { return Device.IsFlashSupported(this); }}
        /// <summary>
        /// Does this camera support torch?
        /// </summary>
        [Doc(@"IsTorchSupported")]
        public bool IsTorchSupported { get { return Device.IsTorchSupported(this); }}
        /// <summary>
        /// Get the camera's horizontal field-of-view
        /// </summary>
        [Doc(@"HorizontalFOV")]
        public float HorizontalFOV { get { return Device.HorizontalFOV(this); }}
        /// <summary>
        /// Get the camera's vertical field-of-view
        /// </summary>
        [Doc(@"VerticalFOV")]
        public float VerticalFOV { get { return Device.VerticalFOV(this); }}
        /// <summary>
        /// Get the camera's minimum exposure bias
        /// </summary>
        [Doc(@"MinExposureBias")]
        public float MinExposureBias { get { return Device.MinExposureBias(this); }}
        /// <summary>
        /// Get the camera's maximum exposure bias
        /// </summary>
        [Doc(@"MaxExposureBias")]
        public float MaxExposureBias { get { return Device.MaxExposureBias(this); }}
        /// <summary>
        /// Get the camera's maximum zoom ratio
        /// </summary>
        [Doc(@"MaxZoomRatio")]
        public float MaxZoomRatio { get { return Device.MaxZoomRatio(this); }}
        #endregion


        #region ---Properties---

        /// <summary>
        /// Get or set the current preview resolution of the camera
        /// </summary>
        [Doc(@"PreviewResolution")]
        public CameraResolution PreviewResolution {
            get { return Device.GetPreviewResolution(this); }
            set { Device.SetPreviewResolution(this, value); }
        }
        /// <summary>
        /// Get or set the current photo resolution of the camera
        /// </summary>
        [Doc(@"PhotoResolution")]
        public CameraResolution PhotoResolution {
            get { return Device.GetPhotoResolution(this); }
            set { Device.SetPhotoResolution(this, value); }
        }
        /// <summary>
        /// Get or set the current framerate of the camera
        /// </summary>
        [Doc(@"Framerate")]
        public float Framerate {
            get { return Device.GetFramerate(this); }
            set { Device.SetFramerate(this, value); }
        }
        /// <summary>
        /// Get or set the camera's focus mode
        /// </summary>
        [Doc(@"CameraFocusMode"), Code(@"ContinuousFocus")]
        public FocusMode FocusMode {
            get { return (FocusMode)Device.GetFocusMode(this); }
            set { Device.SetFocusMode(this, (int)value); }
        }
        /// <summary>
        /// Set the camera's focus point of interest
        /// </summary>
        [Doc(@"FocusPoint", @"FocusPointDiscussion"), Code(@"FocusCamera")]
        public Vector2 FocusPoint {
            get { return Vector2.zero; }
            set { Device.SetFocus(this, value.x, value.y); }
        }
        /// <summary>
        /// Get or set the camera's exposure mode
        /// </summary>
        [Doc(@"CameraExposureMode")]
        public ExposureMode ExposureMode {
            get { return (ExposureMode)Device.GetExposureMode(this); }
            set { Device.SetExposureMode(this, (int)value); }
        }
        /// <summary>
        /// Get or set the camera's exposure bias
        /// </summary>
        [Doc(@"ExposureBias", @"ExposureBiasDiscussion")]
        public float ExposureBias {
            get { return Device.GetExposure(this); }
            set { Device.SetExposure(this, value); }
        }
        /// <summary>
        /// Get or set the camera's flash mode when taking a picture
        /// </summary>
        [Doc(@"CameraFlashMode")]
        public FlashMode FlashMode {
            get { return (FlashMode)Device.GetFlash(this); }
            set { Device.SetFlash(this, (int)value); }
        }
        /// <summary>
        /// Get or set the camera's torch mode
        /// </summary>
        [Doc(@"TorchEnabled")]
        public bool TorchEnabled {
            get { return Device.GetTorchEnabled(this); }
            set { Device.SetTorchEnabled(this, value); }
        }
        /// <summary>
        /// Get or set the camera's current zoom ratio. This value must be between [1, MaxZoomRatio]
        /// </summary>
        [Doc(@"ZoomRatio")]
        public float ZoomRatio {
            get { return Device.GetZoom(this); }
            set { Device.SetZoom(this, value); }
        }
        #endregion


        #region ---Typecasting---
        private static INatCamDevice Device { get { return NatCam.Implementation.Device; }}
        
		public static implicit operator int (DeviceCamera cam) {
			return cam ? cam.index : -1;
		}
        public static implicit operator DeviceCamera (int index) {
            return Cameras.Where(c => c.index == index).FirstOrDefault();
        }
        public static implicit operator bool (DeviceCamera cam) {
            return cam != null;
        }
        #endregion


        #region ---Intializers---

        private readonly int index;

        private DeviceCamera (int i) { index = i; }

        static DeviceCamera () {
            Cameras = new DeviceCamera[WebCamTexture.devices.Length];
            for (int i = 0; i < Cameras.Length; i++) Cameras[i] = new DeviceCamera(i);
            RearCamera = Cameras.FirstOrDefault(c => !c.IsFrontFacing);
            FrontCamera = Cameras.FirstOrDefault(c => c.IsFrontFacing);
        }
        #endregion
        
        
		#region ---Statics---
        [Doc(@"FrontCamera")] public static readonly DeviceCamera FrontCamera;
		[Doc(@"RearCamera")] public static readonly DeviceCamera RearCamera;
        [Doc(@"Cameras"), Code(@"EnableTorch")] public static readonly DeviceCamera[] Cameras; // You shall not touch!
        #endregion
    }
}