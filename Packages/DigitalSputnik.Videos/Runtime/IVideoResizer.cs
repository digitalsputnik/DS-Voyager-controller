namespace DigitalSputnik.Videos
{
    public delegate void VideoResizeHandler(bool success, string error);
    
    public interface IVideoResizer
    {
        void Resize(Video video, int width, int height, VideoResizeHandler resized);
    }
}