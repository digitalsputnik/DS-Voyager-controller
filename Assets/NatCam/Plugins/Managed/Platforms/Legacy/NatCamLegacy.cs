/* 
*   NatCam
*   Copyright (c) 2018 Yusuf Olokoba
*/

namespace NatCamU.Core.Platforms {

    using UnityEngine;
    using System;
    using System.Runtime.InteropServices;
    using Dispatch;

    public sealed class NatCamLegacy : INatCam {

        #region --Events--
        public event PreviewCallback OnStart;
        public event PreviewCallback OnFrame;
        #endregion


        #region --Op vars--
        private int camera = -1;
        private bool firstFrame;
        private IDispatch dispatch;
        private Color32[] previewBuffer;
        #endregion


        #region --Properties--
        public INatCamDevice Device { get; private set; }
        public int Camera {
            get { return camera; }
            set {
                if (IsPlaying) {
                    Pause();
                    camera = value;
                    WebCamTexture.Destroy(PreviewTexture);
                    PreviewTexture = null;
                    Play();
                } else {
                    camera = value;
                    if (PreviewTexture) WebCamTexture.Destroy(PreviewTexture);
                    PreviewTexture = null;
                }
            }
        }
        public WebCamTexture PreviewTexture { get; private set; }
        public Texture Preview { get { return PreviewTexture; }}
        public bool IsInitialized { get { return PreviewTexture; }}
        public bool IsPlaying { get { return PreviewTexture && PreviewTexture.isPlaying; }}
        public bool HasPermissions { get { return true; }}
        #endregion


        #region --Ctor--

        public NatCamLegacy () {
            Device = new NatCamDeviceLegacy();
            Debug.Log("NatCam: Initialized NatCam 2.0 Legacy backend");
        }
        #endregion
        

        #region --Operations--

        public void Play () {
            // Create dispatch
            if (dispatch == null) {
                dispatch = new MainDispatch();
                dispatch.Dispatch(Update);
            }
            // Create preview
            string name = WebCamTexture.devices[camera].name;
            var resolution = Device.GetPreviewResolution(camera);
            var rate = Mathf.Max(30, (int)Device.GetFramerate(camera));
            PreviewTexture = resolution.width == 0 ?  new WebCamTexture(name) : new WebCamTexture(name, resolution.width, resolution.height, rate);
            // Start preview
            Resume();      
        }

        public void Pause () {
            PreviewTexture.Stop();
        }

        public void Resume () {
            firstFrame = true;
            PreviewTexture.Play();
        }

        public void Release () {
            if (!PreviewTexture) return;
            OnStart =
            OnFrame = null;
            PreviewTexture.Stop();
            WebCamTexture.Destroy(PreviewTexture);
            PreviewTexture = null;
            previewBuffer = null;
            dispatch.Dispose();
            dispatch = null;
            camera = -1;
        }

        public void CapturePhoto (PhotoCallback callback) {
            var photo = new Texture2D(PreviewTexture.width, PreviewTexture.height, TextureFormat.RGB24, false, false);
            photo.SetPixels32(PreviewTexture.GetPixels32());
            photo.Apply();
            callback(photo);
        }

        public bool CaptureFrame (Texture2D frame) {
            // Copy into frame
            frame.SetPixels32(previewBuffer);
            frame.Apply();
            return true;
        }

        public bool CaptureFrame (byte[] pixels, bool flip) {
            // Copy
            GCHandle pin = GCHandle.Alloc(previewBuffer, GCHandleType.Pinned);
            if (flip) {
                for (int i = 0, rowSize = PreviewTexture.width * 4; i < PreviewTexture.height; i++)
                    Marshal.Copy((IntPtr)(pin.AddrOfPinnedObject().ToInt64() + i * rowSize), pixels, (PreviewTexture.height - i - 1) * rowSize, rowSize);
            }
            else Marshal.Copy(pin.AddrOfPinnedObject(), pixels, 0, previewBuffer.Length * Marshal.SizeOf(typeof(Color32)));
            pin.Free();
            return true;
        }
        #endregion


        #region --State Management--

        private void Update () {
            // Check that we are playing
            if (
                !PreviewTexture ||
                !PreviewTexture.isPlaying ||
                !PreviewTexture.didUpdateThisFrame ||
                PreviewTexture.width == 16 || 
                PreviewTexture.height == 16
            ) {
                if (dispatch != null) dispatch.Dispatch(Update);
                return;
            }
            // Update preview buffer
            if (previewBuffer == null) previewBuffer = PreviewTexture.GetPixels32();
            else PreviewTexture.GetPixels32(previewBuffer);
            // Invoke events
            if (firstFrame) {
                if (OnStart != null) OnStart();
                firstFrame = false;
            }
            if (OnFrame != null) OnFrame();
            // Re-invoke
            dispatch.Dispatch(Update);
        }
        #endregion
    }
}