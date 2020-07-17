using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

namespace DigitalSputnik.Videos.iOS
{
    public class IosVideoResizer : IVideoResizer
    {
        public bool Resize(Video video, int width, int height)
        {
            var flow = FlowState.Waiting;
            var output = MakeCopyPath(video.Path);
            var listener = CreateListener();
            
            listener.OnError += error =>
            {
                Debug.LogError($"[Video Resizer] failed to resize video: {error}.");
                flow = FlowState.Failed;
            };

            listener.OnResized += path =>
            {
                output = path;
                flow = FlowState.Success;
            };
            
            _iOS_VideoResizer_ResizeVideo(video.Path, output, width, height);

            while (flow == FlowState.Waiting) Thread.Sleep(10);

            CleanListener(listener);

            if (flow != FlowState.Success) return false;
            
            File.Delete(video.Path);
            File.Move(output, video.Path);
            return true;
        }

        private static string MakeCopyPath(string path)
        {
            var extenstion = Path.GetExtension(path);
            var directory = Path.GetDirectoryName(path) ?? "";
            var filename = Path.GetFileNameWithoutExtension(path);
            return Path.Combine(directory, $"{filename}_temp.{extenstion}");
        }

        private static IosVideoResizerListener CreateListener()
        {
            var go = new GameObject("iOS Video Resize Listener");
            var listener = go.AddComponent<IosVideoResizerListener>();
            return listener;
        }

        private static void CleanListener(Component listener)
        {
            Object.Destroy(listener.gameObject);
        }
        
        [DllImport("__Internal")]
        private static extern void _iOS_VideoResizer_ResizeVideo(string path, string output, int width, int height);

        private enum FlowState
        {
            Waiting,
            Failed,
            Success
        }
    }
}