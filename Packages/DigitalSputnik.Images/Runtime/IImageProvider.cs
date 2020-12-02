namespace DigitalSputnik.Images
{
    public delegate void ImageHandler(Image image);
    
    public interface IImageProvider
    {
        void LoadImage(string path, ImageHandler loaded);
    }
}