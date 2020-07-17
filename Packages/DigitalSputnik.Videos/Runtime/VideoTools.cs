using System.Threading;

namespace DigitalSputnik.Videos
{
    public delegate void VideoHandler(Video video);
    
    public class VideoTools
    {
        public int LoadVideoSleepMilliseconds { get; set; } = 10;
        public int LoadVideoTimeoutMilliseconds { get; set; } = 5000;
        
        private readonly IVideoProvider _provider;
        private readonly IVideoResizer _resizer;
        private readonly ITimeProvider _time;

        public VideoTools(IVideoProvider provider, IVideoResizer resizer, ITimeProvider time)
        {
            _provider = provider;
            _resizer = resizer;
            _time = time;
        }

        public Video LoadVideo(string path)
        {
            var timeout = _time.Epoch + (double) LoadVideoTimeoutMilliseconds / 1000;
            Video video = null;
            var loaded = false;
            
            _provider.LoadVideo(path, vid =>
            {
                video = vid;
                loaded = true;
            });

            while (!loaded && _time.Epoch < timeout)
                Thread.Sleep(LoadVideoSleepMilliseconds);
            
            return video;
        }

        public bool Rename(ref Video video, string name) => _provider.Rename(ref video, name);

        public bool Resize(ref Video video, int width, int height)
        {
            return _resizer.Resize(ref video, width, height);
        }
    }
}