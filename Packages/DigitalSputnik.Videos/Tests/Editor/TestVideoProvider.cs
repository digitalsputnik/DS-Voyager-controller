namespace DigitalSputnik.Videos.Tests
{
    internal class TestVideoProvider : IVideoProvider
    {
        public void LoadVideo(string path, VideoHandler loaded)
        {
            if (path.EndsWith("h_chase.mp4"))
            {
                var video = new Video
                {
                    Name = "h_chase",
                    Path = path,
                    Width = 640,
                    Height = 360,
                    Fps = 30,
                    FrameCount = 150
                };
                loaded?.Invoke(video, null);
            }
            else loaded?.Invoke(null, null);
        }

        public bool Rename(ref Video video, string name)
        {
            if (!IsNameCorrect(name)) return false;

            // This is not a correct way to do this. If implementing for real use Path helpers!
            video.Path = video.Path.Replace(video.Name, name);
            video.Name = name;
            return true;
        }

        public bool IsNameCorrect(string name)
        {
            return true;
        }
    }
}