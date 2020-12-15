namespace DigitalSputnik.Videos
{
    public class Video
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public ulong FrameCount { get; set; }
        public double Fps { get; set; }
        public double Duration => FrameCount / Fps;
    }
}