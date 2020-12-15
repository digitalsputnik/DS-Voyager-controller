namespace DigitalSputnik.Images
{
    public interface IImageRenamer
    {
        bool Rename(ref Image video, string name);
    }
}