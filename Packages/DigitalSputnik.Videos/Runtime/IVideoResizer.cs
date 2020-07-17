namespace DigitalSputnik.Videos
{
    public interface IVideoResizer
    {
        bool Resize(ref Video video, int width, int height);
    }
}