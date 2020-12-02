namespace DigitalSputnik.Images
{
    public delegate void ImageResizeHandler(bool success, string error);
    
    public interface IImageResizer
    {
        void Resize(Image image, int width, int height, ImageResizeHandler resized);
    }
}