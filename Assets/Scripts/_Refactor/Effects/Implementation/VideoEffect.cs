using DigitalSputnik.Videos;

namespace VoyagerApp.RefactoredEffects
{
    public class VideoEffect : Effect
    {
        public override string Name => Video.Name;
        public Video Video { get; }

        public VideoEffect(Video video)
        {
            Video = video;
        }
    }
}