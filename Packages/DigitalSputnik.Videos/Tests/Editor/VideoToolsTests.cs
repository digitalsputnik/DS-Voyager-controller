using System.Diagnostics;
using System.IO;
using NUnit.Framework;

namespace DigitalSputnik.Videos.Tests
{
    [TestFixture]
    public class VideoToolsTests
    {
        [Test]
        public void VideoExists()
        {
            Assert.IsTrue(File.Exists(A.TestVideoPath));
        }

        [Test]
        public void VideoLoads()
        {
            Assert.IsNotNull(A.VideoTools.GetTestVideo());
        }
        
        [Test]
        public void VideoNameMatches()
        {
            Assert.AreEqual(A.VideoTools.GetTestVideo()?.Name, A.TEST_VIDEO_NAME);
        }
        
        [Test]
        public void VideoWidthMatches()
        {
            Assert.AreEqual(A.VideoTools.GetTestVideo()?.Width, A.TEST_VIDEO_WIDTH);
        }
        
        [Test]
        public void VideoHeightMatches()
        {
            Assert.AreEqual(A.VideoTools.GetTestVideo()?.Height, A.TEST_VIDEO_HEIGHT);
        }

        [Test]
        public void VideoRenameChangesName()
        {
            const string NEW_NAME = "New Name";
            
            var tools = A.VideoTools;
            var video = tools.GetTestVideo();

            tools.Rename(ref video, NEW_NAME);
            Assert.AreEqual(video.Name, NEW_NAME);
        }

        [Test]
        public void VideoRenameChangesPath()
        {
            const string NEW_NAME = "New Name";
            
            var newPath = A.TestVideoPath.Replace(A.TEST_VIDEO_NAME, NEW_NAME);
            var tools = A.VideoTools;
            var video = tools.GetTestVideo();

            tools.Rename(ref video, NEW_NAME);
            Assert.AreEqual(video.Path, newPath);
        }

        [Test]
        public void ResizingVideoChangesWidth()
        {
            const int TARGET_WIDTH = 320;
            const int TARGET_HEIGHT = 180;
            
            var tools = A.VideoTools;
            var video = tools.GetTestVideo();

            tools.Resize(ref video, TARGET_WIDTH, TARGET_HEIGHT);
            Assert.AreEqual(video.Width, TARGET_WIDTH);
        }

        [Test]
        public void ResizingVideoChangesHeight()
        {
            const int TARGET_WIDTH = 320;
            const int TARGET_HEIGHT = 180;
            
            var tools = A.VideoTools;
            var video = tools.GetTestVideo();

            tools.Resize(ref video, TARGET_WIDTH, TARGET_HEIGHT);
            Assert.AreEqual(video.Height, TARGET_HEIGHT);
        }
    }

    internal static class A
    {
        public const string TEST_VIDEO_NAME = "h_chase";
        public const int TEST_VIDEO_WIDTH = 640;
        public const int TEST_VIDEO_HEIGHT = 360;

        private static IVideoProvider VideoProvider => new TestVideoProvider();
        private static IVideoResizer VideoResizer => new TestVideoResizer();
        private static ITimeProvider TimeProvider => new SystemTimeProvider();
        public static VideoTools VideoTools => new VideoTools(VideoProvider, VideoResizer, TimeProvider);
        public static Video GetTestVideo(this VideoTools tools) => tools.LoadVideo(TestVideoPath);

        public static string TestVideoPath
        {
            get
            {
                var traceFileName = new StackTrace(true).GetFrame(0).GetFileName();
                var directory = Path.GetDirectoryName(traceFileName) ?? "";
                return Path.Combine(directory, TEST_VIDEO_NAME + ".mp4");
            }
        }
    }
}
