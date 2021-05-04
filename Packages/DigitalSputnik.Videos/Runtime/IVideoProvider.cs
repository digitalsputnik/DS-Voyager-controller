using UnityEngine;

namespace DigitalSputnik.Videos
{
<<<<<<< HEAD
    public delegate void VideoHandler(Video video);
    
=======
    public delegate void VideoHandler(Video video, Texture2D thumbnail);
>>>>>>> v2.6
    public interface IVideoProvider
    {
        void LoadVideo(string path, VideoHandler loaded);
        bool Rename(ref Video video, string name);
        bool IsNameCorrect(string name);
    }
}