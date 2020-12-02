namespace DigitalSputnik.Images
{
    public class ImageTools
    {
        private readonly IImageProvider _provider;
        private readonly IImageRenamer _renamer;
        private readonly IImageResizer _resizer;
        
        public ImageTools(IImageProvider provider, IImageRenamer renamer, IImageResizer resizer)
        {
            _provider = provider;
            _renamer = renamer;
            _resizer = resizer;
        }

        public void Load(string path, ImageHandler loaded)
        {
            if (_provider != null)
                _provider.LoadImage(path, loaded);
            else
                loaded?.Invoke(null);
        }
        
        public bool Rename(ref Image image, string name) => _renamer?.Rename(ref image, name) ?? false;

        public void Resize(Image image, int width, int height, ImageResizeHandler resized)
        {
            if (_resizer != null)
            {
                _resizer.Resize(image, width, height, resized);
            }
            else
            {
                resized?.Invoke(false, "No resizer implemented");
            }
        }
    }
}