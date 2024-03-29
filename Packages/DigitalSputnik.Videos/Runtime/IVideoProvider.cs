using UnityEngine;

namespace DigitalSputnik.Videos
{
    public delegate void VideoHandler(Video video, Texture2D thumbnail);

    public interface IVideoProvider
    {
        void LoadVideo(string path, VideoHandler loaded);
        bool Rename(ref Video video, string name);
        bool IsNameCorrect(string name);
    }
}