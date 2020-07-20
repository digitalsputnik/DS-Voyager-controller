using System.Threading;

namespace DigitalSputnik.Videos
{
    public delegate void VideoHandler(Video video);
    
    public class VideoTools
    {
        private readonly IVideoProvider _provider;
        private readonly IVideoResizer _resizer;

        public VideoTools(IVideoProvider provider, IVideoResizer resizer)
        {
            _provider = provider;
            _resizer = resizer;
        }

        public void LoadVideo(string path, VideoHandler loaded)
        {
            _provider.LoadVideo(path, loaded);
        }

        public bool Rename(ref Video video, string name) => _provider.Rename(ref video, name);

        public bool Resize(ref Video video, int width, int height)
        {
            return _resizer.Resize(ref video, width, height);
        }
    }
}