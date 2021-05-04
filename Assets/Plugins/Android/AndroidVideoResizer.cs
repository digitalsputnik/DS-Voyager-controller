#if UNITY_ANDROID && !UNITY_EDITOR
using DigitalSputnik;
using DigitalSputnik.Videos;
using System;
using System.IO;
using System.Threading;
using UnityEngine;

public class AndroidVideoResizer : IVideoResizer
{
    const string PLUGIN_CLASS_NAME = "com.example.videoresizer.VideoResizer";

    private static AndroidJavaObject _pluginInstance;

    private static bool initialized = false;
    private static bool callbacksSet = false;

    private static VideoResizeHandler _callback;
    private static Video _resizingVideo;

    public static string Progress = "";
    public static bool IsBusy 
    {
        get
        {
            if (_resizingVideo == null)
                return false;
            else return true;
        }
    }

    static AndroidJavaObject PluginInstance
    {
        get
        {
            if (_pluginInstance == null)
                _pluginInstance = new AndroidJavaObject(PLUGIN_CLASS_NAME);

            return _pluginInstance;
        }
    }

    static void Initialize()
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject context = activity.Call<AndroidJavaObject>("getApplicationContext");
        AndroidJavaObject application = activity.Call<AndroidJavaObject>("getApplication");

        object[] variables =
        {
            context,
            activity,
            application
        };

        PluginInstance.CallStatic("initialize", variables);

        initialized = true;
    }

    static void SetCallbacks()
    {
        PluginInstance.CallStatic("setCallbacks", new AndroidVideoResizingCallback(ResizingStarted,
                                                                                   ResizingProgress,
                                                                                   ResizingSuccess,
                                                                                   ResizingFailed,
                                                                                   ResizingCancelled));

        callbacksSet = true;
    }

    public void Resize(Video video, int width, int height, VideoResizeHandler resized)
    {
        if (IsBusy == false)
        {
            _callback = resized;
            _resizingVideo = video;

            ResizeThread(video.Path);
        }
        else
            _callback?.Invoke(false, "VideoResizer is busy");
    }

    public static void UpdateCallback(VideoResizeHandler resized) => _callback = resized;

    private static void ResizeThread(string pickedVideoPath)
    {
        if (!initialized)
            Initialize();

        while (!initialized) Thread.Sleep(10);

        if (!callbacksSet)
            SetCallbacks();

        while (!callbacksSet) Thread.Sleep(10);

        var output = Path.GetDirectoryName(pickedVideoPath);

        object[] parameters = { pickedVideoPath, output };

        PluginInstance.CallStatic("resizeVideo", parameters);
    }

    public static void ResizingStarted(long _startTime, string _startSize)
    {
        Debug.Log("Resizing Started");
    }

    public static void ResizingProgress(string _progress)
    {
        if (_progress != Progress) 
        {
            Progress = _progress;

            Debug.Log("Resizing Progress: " + _progress);
        }
    }

    public static void ResizingSuccess(long _resizingEnded, string _resizingDuration, string _newSize, string _path)
    {
        File.Delete(_resizingVideo.Path);
        File.Move(_path, _resizingVideo.Path);

        var originalHeight = _resizingVideo.Height;
        var originalWidth = _resizingVideo.Width;

        int newHeight;
        int newWidth;

        if (originalWidth > originalHeight)
        {
            newWidth = 512;
            double ratio = (double)newWidth / (double)originalWidth;
            newHeight = Convert.ToInt32(Math.Round(Convert.ToDecimal(originalHeight * ratio / 16))) * 16;
        }
        else
        {
            newHeight = 512;
            double ratio = (double)newHeight / (double)originalHeight;
            newWidth = Convert.ToInt32(Math.Round(Convert.ToDecimal(originalWidth * ratio / 16))) * 16;
        }

        _resizingVideo.Width = newWidth;
        _resizingVideo.Height = newHeight;

        _callback?.Invoke(true, "");

        _resizingVideo = null;
        _callback = null;
        Progress = "";
    }

    public static void ResizingFailed(string _error)
    {
        _callback?.Invoke(false, _error);

        _resizingVideo = null;
        _callback = null;
        Progress = "";
    }

    public static void ResizingCancelled(string _error)
    {
        _callback?.Invoke(false, _error);

        _resizingVideo = null;
        _callback = null;
        Progress = "";
    }

    class AndroidVideoResizingCallback : AndroidJavaProxy
    {
        internal Action<long, string> _startCallback;
        internal Action<string> _progressCallback;
        internal Action<long, string, string, string> _successCallback;
        internal Action<string> _failCallback;
        internal Action<string> _cancelCallback;

        public AndroidVideoResizingCallback() : base("com.example.videoresizer.VideoResizerCallback") { }

        public AndroidVideoResizingCallback(Action<long, string> startCallback,
                                            Action<string> progressCallback,
                                            Action<long, string, string, string> successCallback,
                                            Action<string> failCallback,
                                            Action<string> cancelCallback) : this()
        {
            _startCallback = startCallback;
            _progressCallback = progressCallback;
            _successCallback = successCallback;
            _failCallback = failCallback;
            _cancelCallback = cancelCallback;
        }

        public void onVideoResizingStarted(long startTime, string startSize)
        {
            _startCallback?.Invoke(startTime, startSize);
        }

        public void onVideoProgress(string progress)
        {
            _progressCallback?.Invoke(progress);
        }

        public void onVideoResized(long resizingEnded, string resizingDuration, string newSize, string path)
        {
            _successCallback?.Invoke(resizingEnded, resizingDuration, newSize, path);
        }

        public void onVideoResizingFailed(string error)
        {
            _failCallback?.Invoke(error);
        }

        public void onVideoResizingCancelled(string error)
        {
            _cancelCallback?.Invoke(error);
        }
    }
}
#endif