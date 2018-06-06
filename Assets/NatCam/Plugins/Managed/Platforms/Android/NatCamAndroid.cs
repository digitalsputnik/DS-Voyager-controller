/* 
*   NatCam
*   Copyright (c) 2018 Yusuf Olokoba
*/

namespace NatCamU.Core.Platforms {

    using UnityEngine;
    using System;
    using System.Runtime.InteropServices;
    using Dispatch;

    public sealed class NatCamAndroid : AndroidJavaProxy, INatCam {

        #region --Events--
        public event PreviewCallback OnStart;
        public event PreviewCallback OnFrame;
        #endregion


        #region --Op vars--
        private Texture2D preview;
        private IDispatch dispatch;
        private PhotoCallback photoCallback;
        private readonly AndroidJavaObject natcam;
        /// <summary>
        /// This flag determines if NatCam's preview data pipeline is enabled
        /// The preview data pipeline is what powers the PreviewBuffer, PreviewFrame, and PreviewMatrix API's
        /// On the Galaxy S7, there is a graphics bug that causes the preview data pipeline to lag considerably
        /// </summary>
        private const bool PreviewData = true;
        #endregion


        #region --Properties--
        public INatCamDevice Device {get; private set;}
        public int Camera {
            get { return natcam.Call<int>("getCameraIndex"); }
            set {
                if (IsPlaying) {
                    Pause();
                    natcam.Call("setCamera", value);
                    Play();
                } else natcam.Call("setCamera", value);
            }
        }
        public Texture Preview { get { return preview;}}
        public bool IsInitialized { get {return natcam.Call<bool>("isInitialized"); }}
        public bool IsPlaying { get { return natcam.Call<bool>("isPlaying"); }}
        public bool HasPermissions { get { return natcam.Call<bool>("hasPermissions"); }}
        #endregion


        #region --Ctor--

        public NatCamAndroid () : base("com.yusufolokoba.natcam.NatCamDelegate") {
            natcam = new AndroidJavaObject("com.yusufolokoba.natcam.NatCam", this, PreviewData);
            Device = new NatCamDeviceAndroid();
            RenderDispatch.Initialize();
            DispatchUtility.onPause += OnPause;
            OrientationUtility.onOrient += OnOrient;
            Debug.Log("NatCam: Initialized NatCam 2.0 Android backend");
        }
        #endregion
        

        #region --Operations--

        public void Play () {
            dispatch = dispatch ?? new MainDispatch();
            OnOrient();
            natcam.Call("play");
        }

        public void Pause () {
            natcam.Call("pause");
        }

        public void Resume () {
            natcam.Call("resume");
        }

        public void Release () {
            OnStart = 
            OnFrame = null;
            natcam.Call("release");
            if (preview != null) Texture2D.Destroy(preview); preview = null;
            if (dispatch != null) dispatch.Dispose(); dispatch = null;
        }

        public void CapturePhoto (PhotoCallback callback) {
            photoCallback = callback;
            natcam.Call("capturePhoto");
        }

        public bool CaptureFrame (Texture2D frame) {
            // Get buffer info
            IntPtr bufferPtr = (IntPtr)natcam.Call<long>("captureFrame");
            if (bufferPtr == IntPtr.Zero) return false;
            // Copy
            frame.LoadRawTextureData(bufferPtr, preview.width * preview.height * 4);
            frame.Apply();
            return true;
        }

        public bool CaptureFrame (byte[] pixels, bool flip) {
            // Get buffer handle
            AndroidJNI.AttachCurrentThread();
            var bufferPtr = (IntPtr)natcam.Call<long>("captureFrame");
            if (bufferPtr == IntPtr.Zero) return false;
            // Copy
            if (flip) {
                for (int i = 0, rowSize = preview.width * 4; i < preview.height; i++)
                    Marshal.Copy((IntPtr)(bufferPtr.ToInt64() + i * rowSize), pixels, (preview.height - i - 1) * rowSize, rowSize);
            }
            else Marshal.Copy(bufferPtr, pixels, 0, preview.width * preview.height * 4);
            return true;
        }
        #endregion


        #region --Callbacks--

        private void onStart (int texPtr, int width, int height) {
            dispatch.Dispatch(() => {
                preview = preview ?? Texture2D.CreateExternalTexture(width, height, TextureFormat.RGBA32, false, false, (IntPtr)texPtr);
                if (preview.width != width || preview.height != height) preview.Resize(width, height, preview.format, false);
                if (OnStart != null) OnStart();
            });
        }

        private void onFrame (int texPtr) {
            dispatch.Dispatch(() => {
                if (preview == null) return;
                preview.UpdateExternalTexture((IntPtr)texPtr);
                if (OnFrame != null) OnFrame();
            });
        }

        private void onPhoto (AndroidJavaObject photo) {
            int width = photo.Get<int>("width");
            int height = photo.Get<int>("height");
            byte[] pixelData = AndroidJNI.FromByteArray(photo.Get<AndroidJavaObject>("pixelBuffer").GetRawObject());
            dispatch.Dispatch(() => {
                var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
                texture.LoadRawTextureData(pixelData);
                texture.Apply();
                photoCallback(texture);
            });
        }
        #endregion


        #region --Utility--
        
        private void OnPause (bool paused) {
            natcam.Call("onPause", paused);
        }

        private void OnOrient () {
            natcam.Call("onOrient", OrientationUtility.Orientation);
        }
        #endregion
    }
}