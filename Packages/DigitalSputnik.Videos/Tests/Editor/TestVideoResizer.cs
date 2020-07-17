namespace DigitalSputnik.Videos.Tests
{
    internal class TestVideoResizer : IVideoResizer
    {
        public bool Resize(ref Video video, int width, int height)
        {
            video.Width = width;
            video.Height = height;
            return true;
        }
    }
}