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
                    Height = 360
                };
                loaded?.Invoke(video);
            }
            else loaded?.Invoke(null);
        }

        public bool Rename(Video video, string name)
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