using System.Diagnostics;
using System.IO;
using System.Threading;
using NUnit.Framework;
using UnityEngine;

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
        public void VideoFpsMatches()
        {
            Assert.AreEqual(A.VideoTools.GetTestVideo()?.Fps, A.TEST_VIDEO_FPS);
        }
        
        [Test]
        public void VideoFrameCountMatches()
        {
            Assert.AreEqual(A.VideoTools.GetTestVideo()?.FrameCount, A.TEST_VIDEO_FRAME_COUNT);
        }
        
        [Test]
        public void VideoDurationMatches()
        {
            Assert.AreEqual(A.VideoTools.GetTestVideo()?.Duration, A.TEST_VIDEO_DURATION);
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
        public const int TEST_VIDEO_FPS = 30;
        public const int TEST_VIDEO_FRAME_COUNT = 150;
        public const int TEST_VIDEO_DURATION = 5;

        private static IVideoProvider VideoProvider => new TestVideoProvider();
        private static IVideoResizer VideoResizer => new TestVideoResizer();
        private static ITimeProvider TimeProvider => new SystemTimeProvider();
        public static VideoTools VideoTools => new VideoTools(VideoProvider, VideoResizer);

        public static Video GetTestVideo(this VideoTools tools)
        {
            Video video = null;
            var time = TimeProvider;
            var timeout = time.Epoch + 5.0f;
            
            tools.LoadVideo(TestVideoPath, vid => video = vid);

            while (video == null && time.Epoch < timeout)
                Thread.Sleep(10);

            return video;
        }

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
