#if UNITY_ANDROID //&& !UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using UnityEngine;

public static class AndroidVideoResizer
{
    const string PLUGIN_CLASS_NAME = "com.example.videoresizer.VideoResizer";

    static AndroidJavaObject _pluginInstance;
    static CompressedVideo compressedVideo;
    static Action<bool, string> _compressionFinishedCallback;

    static bool initialized = false;
    static bool callbacksSet = false;

    public static string Progress
    {
        get
        {
            if (compressedVideo != null)
                return compressedVideo.CompressionProgress;

            else return "Nothing is compressing";
        }
    }

    public static bool IsCompressing
    {
        get
        {
            if (compressedVideo == null)
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

    public static void PickVideo(MonoBehaviour source, string pickedVideoPath, Action<bool, string> _callback)
    {
        if (compressedVideo == null)
        {
            _compressionFinishedCallback = _callback;

            source.StartCoroutine(SetupAndPickVideo(pickedVideoPath));
        }
        else
            _callback?.Invoke(false, "Compression in progress");
    }

    public static void UpdateCallback(Action<bool, string> _callback) => _compressionFinishedCallback = _callback;

    static IEnumerator SetupAndPickVideo(string pickedVideoPath)
    {
        if (!initialized)
            Initialize();

        yield return new WaitUntil(() => initialized);

        if (!callbacksSet)
            SetCallbacks();

        yield return new WaitUntil(() => callbacksSet);

        var path = Application.temporaryCachePath + "/imported_videos";

        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        object[] parameters = { pickedVideoPath, path }; 

        PluginInstance.CallStatic("pickVideo", parameters);
    }

    public static void ResizingStarted(long _startTime, string _startSize)
    {
        compressedVideo = new CompressedVideo(_startSize, _startTime);
    }

    public static void ResizingProgress(string _progress)
    {
        compressedVideo.ProgressUpdate(_progress);
    }

    public static void ResizingSuccess(long _resizingEnded, string _resizingDuration, string _newSize, string _path)
    {
        compressedVideo.CompressionFinished(_resizingEnded, _resizingDuration, _newSize, _path);

        _compressionFinishedCallback?.Invoke(true, _path);

        compressedVideo = null;
    }

    public static void ResizingFailed(string _error)
    {
        _compressionFinishedCallback?.Invoke(false, _error);

        compressedVideo = null;
    }

    public static void ResizingCancelled(string _error)
    {
        _compressionFinishedCallback?.Invoke(false, _error);

        compressedVideo = null;
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

public class CompressedVideo
{
    public string StartSize;
    public long CompressionStarted;
    public string CompressionProgress;
    public long CompressionEnded;
    public string CompressionDuration;
    public string NewVideoSize;
    public string NewVideoPath;

    public CompressedVideo(string _startSize, long _startTime)
    {
        StartSize = _startSize;
        CompressionStarted = _startTime;
    }

    public void ProgressUpdate(string _progress) => CompressionProgress = _progress;

    public void CompressionFinished(long _resizingEnded, string _resizingDuration, string _newSize, string _path)
    {
        CompressionEnded = _resizingEnded;
        CompressionDuration = _resizingDuration;
        NewVideoSize = _newSize;
        NewVideoPath = _path;
    }
}
#endif