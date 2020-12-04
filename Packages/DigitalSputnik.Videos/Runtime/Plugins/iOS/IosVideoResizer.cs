#if UNITY_IOS
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DigitalSputnik.Videos.iOS
{
    public class IosVideoResizer : IVideoResizer
    {
        public void Resize(Video video, int width, int height, VideoResizeHandler resized)
        {
            new Thread(() => ResizeThread(video, width, height, resized)).Start();
        }

        public static void ResizeFromPath(string path, int maxWidth, int maxHeight, Action<bool, string> resized)
        {
            var flow = FlowState.Waiting;
            var output = MakeCopyPath(path);
            var listener = CreateListener();
            var error = string.Empty;
            
            listener.OnError += err =>
            {
                error = err;
                flow = FlowState.Failed;
            };

            listener.OnResized += _ => flow = FlowState.Success;
            
            _iOS_VideoResizer_ResizeVideoBetween(path, output, maxWidth, maxHeight);

            while (flow == FlowState.Waiting) Thread.Sleep(10);

            CleanListener(listener);
            
            if (flow != FlowState.Success)
                resized?.Invoke(false, error);
            
            File.Delete(path);
            File.Move(output, path);
            
            MainThreadRunner.Instance.EnqueueAction(() => resized?.Invoke(true, string.Empty));
        }

        private static void ResizeThread(Video video, int width, int height, VideoResizeHandler resized)
        {
            var flow = FlowState.Waiting;
            var output = MakeCopyPath(video.Path);
            var listener = CreateListener();
            var error = string.Empty;
            
            listener.OnError += err =>
            {
                error = err;
                flow = FlowState.Failed;
            };

            listener.OnResized += path => flow = FlowState.Success;
            
            _iOS_VideoResizer_ResizeVideo(video.Path, output, width, height);

            while (flow == FlowState.Waiting) Thread.Sleep(10);

            CleanListener(listener);

            if (flow != FlowState.Success)
                resized?.Invoke(false, error);
            
            File.Delete(video.Path);
            File.Move(output, video.Path);

            video.Width = width;
            video.Height = height;

            MainThreadRunner.Instance.EnqueueAction(() => resized?.Invoke(true, string.Empty));
        }

        private static string MakeCopyPath(string path)
        {
            var extenstion = Path.GetExtension(path);
            var directory = Path.GetDirectoryName(path) ?? "";
            var filename = Path.GetFileNameWithoutExtension(path);
            return Path.Combine(directory, $"{filename}_temp{extenstion}");
        }

        private static IosVideoResizerListener CreateListener()
        {
            GameObject go = null;
            IosVideoResizerListener listener = null;
            MainThreadRunner.Instance.EnqueueAction(() =>
            {
               go = new GameObject("iOS Video Resize Listener");
               listener = go.AddComponent<IosVideoResizerListener>();
            });
            
            while (listener == null)
                Thread.Sleep(10);
            
            return listener;
        }

        private static void CleanListener(Component listener)
        {
            MainThreadRunner.Instance.EnqueueAction(() => Object.Destroy(listener.gameObject));
        }

        [DllImport("__Internal")]
        private static extern void _iOS_VideoResizer_ResizeVideo(string path, string output, int width, int height);
        
        [DllImport("__Internal")]
        private static extern void _iOS_VideoResizer_ResizeVideoBetween(string path, string output, int width, int height);

        private enum FlowState
        {
            Waiting,
            Failed,
            Success
        }
    }
}
#endif