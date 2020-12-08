#if UNITY_IOS
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

namespace DigitalSputnik.Images.iOS
{
    public class IosImageResizer : IImageResizer
    {
        private static IosImageResizer _instance;
        private static IosImageResizerListener _listener;
        
        public static void Initialize()
        {
            if (_instance == null)
            {
                _instance = new IosImageResizer();
                _listener = CreateListener();
            }
        }

        public static void Destroy() => CleanListener(_listener);

        public void Resize(Image image, int width, int height, ImageResizeHandler resized)
        {
            var flow = FlowState.Waiting;
            var output = MakeCopyPath(image.Path);
            var error = string.Empty;

            void Success(string path)
            {
                flow = FlowState.Success;
                output = path;
                Debug.Log(output);
            }

            _listener.OnResized += Success;
            
            _iOS_ImageResizer_ResizeImageBetween(image.Path, output, width, height);

            while (flow == FlowState.Waiting) Thread.Sleep(10);
            
            if (flow != FlowState.Success)
                resized?.Invoke(false, error);

            _listener.OnResized -= Success;
            
            File.Delete(image.Path);
            File.Move(output, image.Path);
            
            MainThreadRunner.Instance.EnqueueAction(() => resized?.Invoke(true, string.Empty));
        }
        
        private static string MakeCopyPath(string path)
        {
            var extenstion = Path.GetExtension(path);
            var directory = Path.GetDirectoryName(path) ?? "";
            var filename = Path.GetFileNameWithoutExtension(path);
            return Path.Combine(directory, $"{filename}_temp{extenstion}");
        }
        
        private static IosImageResizerListener CreateListener()
        {
            GameObject go = null;
            IosImageResizerListener listener = null;
            MainThreadRunner.Instance.EnqueueAction(() =>
            {
                go = new GameObject("iOS Image Resize Listener");
                listener = go.AddComponent<IosImageResizerListener>();
            });
            
            while (listener == null)
                Thread.Sleep(10);
            
            return listener;
        }

        private static void CleanListener(Component listener)
        {
            MainThreadRunner.Instance.EnqueueAction(() => Object.Destroy(listener.gameObject));
        }

        private enum FlowState
        {
            Waiting,
            Success
        }

        [DllImport("__Internal")]
        private static extern void _iOS_ImageResizer_ResizeImageBetween(string path, string output, int width, int height);
    }
}
#endif