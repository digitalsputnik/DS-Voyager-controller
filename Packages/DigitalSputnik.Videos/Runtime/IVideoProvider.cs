namespace DigitalSputnik.Videos
{
    public interface IVideoProvider
    {
        void LoadVideo(string path, VideoHandler loaded);
        bool Rename(ref Video video, string name);
        bool IsNameCorrect(string name);
    }
}