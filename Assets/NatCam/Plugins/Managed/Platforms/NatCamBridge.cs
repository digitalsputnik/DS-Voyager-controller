/* 
*   NatCam
*   Copyright (c) 2018 Yusuf Olokoba
*/

namespace NatCamU.Core.Platforms {

    using System;
    using System.Runtime.InteropServices;

    public static partial class NatCamBridge {

        private const string Assembly =
        #if UNITY_IOS
        "__Internal";
        #else
        "NatCam";
        #endif

        #region ---Delegates---
        public delegate void StartCallback (IntPtr texPtr, int width, int height);
        public delegate void PreviewCallback (IntPtr texPtr);
        public delegate void PhotoCallback (IntPtr imgPtr, int width, int height, int size);
        #endregion
        
        #if UNITY_IOS && !UNITY_EDITOR

        #region --Operations--
        [DllImport(Assembly, EntryPoint = "NCCoreRegisterCallbacks")]
        public static extern void RegisterCoreCallbacks (StartCallback startCallback,  PreviewCallback previewCallback, PhotoCallback photoCallback);
        [DllImport(Assembly, EntryPoint = "NCCoreGetCamera")]
        public static extern int GetCamera ();
        [DllImport(Assembly, EntryPoint = "NCCoreSetCamera")]
        public static extern void SetCamera (int camera);
        [DllImport(Assembly, EntryPoint = "NCCoreIsInitialized")]
        public static extern bool IsInitialized ();
        [DllImport(Assembly, EntryPoint = "NCCoreIsPlaying")]
        public static extern bool IsPlaying ();
        [DllImport(Assembly, EntryPoint = "NCCorePlay")]
        public static extern void Play ();
        [DllImport(Assembly, EntryPoint = "NCCorePause")]
        public static extern void Pause ();
        [DllImport(Assembly, EntryPoint = "NCCoreResume")]
        public static extern void Resume ();
        [DllImport(Assembly, EntryPoint = "NCCoreRelease")]
        public static extern void Release ();
        [DllImport(Assembly, EntryPoint = "NCCoreCapturePhoto")]
        public static extern void CapturePhoto ();
        [DllImport(Assembly, EntryPoint = "NCCoreReleasePhoto")]
        public static extern void ReleasePhoto ();
        [DllImport(Assembly, EntryPoint = "NCCoreCaptureFrame")]
        public static extern void CaptureFrame (out IntPtr ptr);
        [DllImport(Assembly, EntryPoint = "NCCoreInvertFrame")]
        public static extern void InvertFrame (IntPtr dest);
        [DllImport(Assembly, EntryPoint = "NCCoreGetOrientation")]
        public static extern byte GetOrientation ();
        [DllImport(Assembly, EntryPoint = "NCCoreSetOrientation")]
        public static extern void SetOrientation (int orientation);
        #endregion

        #region --Utility--
        [DllImport(Assembly, EntryPoint = "NCCoreOnPause")]
        public static extern void OnPause (bool paused);
        [DllImport(Assembly, EntryPoint = "NCCoreHasPermissions")]
        public static extern bool HasPermissions ();
        #endregion


        #else
        public static void RegisterCoreCallbacks (StartCallback startCallback,  PreviewCallback previewCallback, PhotoCallback photoCallback) {}
        public static int GetCamera () { return -1; }
        public static void SetCamera (int camera) {}
        public static bool IsInitialized () { return false; }
        public static bool IsPlaying () { return false; }
        public static void Play () {}
        public static void Pause () {}
        public static void Resume () {}
        public static void Release () {}
        public static void CapturePhoto () {}
        public static void ReleasePhoto () {}
        public static void CaptureFrame (out IntPtr ptr) { ptr = IntPtr.Zero; }
        public static void InvertFrame (IntPtr dest) {}
        public static byte GetOrientation () { return 0; }
        public static void SetOrientation (int orientation) {}
        public static void OnPause (bool paused) {}
        public static bool HasPermissions () { return false; }
        #endif
    }
}