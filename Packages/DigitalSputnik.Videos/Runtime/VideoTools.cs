namespace DigitalSputnik.Videos
{
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

        public void Rename(ref Video video, string name) => _provider.Rename(ref video, name);
        public void Resize(Video video, int width, int height, VideoResizeHandler resized) => _resizer.Resize(video, width, height, resized);
    }
}