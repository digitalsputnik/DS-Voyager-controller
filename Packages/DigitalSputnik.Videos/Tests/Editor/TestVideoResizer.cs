namespace DigitalSputnik.Videos.Tests
{
    internal class TestVideoResizer : IVideoResizer
    {
        public void Resize(Video video, int width, int height, VideoResizeHandler resized)
        {
            video.Width = width;
            video.Height = height;
            resized?.Invoke(true, string.Empty);
        }
    }
}