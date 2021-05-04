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
            Assert.IsNotNull(A.VideoTools.GetTestVideo().Video);
        }

        [Test]
        public void ThumbnailLoads()
        {
            Assert.IsNotNull(A.VideoTools.GetTestVideo().Thumbnail);
        }

        [Test]
        public void VideoNameMatches()
        {
            Assert.AreEqual(A.VideoTools.GetTestVideo()?.Video?.Name, A.TEST_VIDEO_NAME);
        }
        
        [Test]
        public void VideoWidthMatches()
        {
            Assert.AreEqual(A.VideoTools.GetTestVideo()?.Video?.Width, A.TEST_VIDEO_WIDTH);
        }
        
        [Test]
        public void VideoHeightMatches()
        {
            Assert.AreEqual(A.VideoTools.GetTestVideo()?.Video?.Height, A.TEST_VIDEO_HEIGHT);
        }

        [Test]
        public void VideoFpsMatches()
        {
            Assert.AreEqual(A.VideoTools.GetTestVideo()?.Video?.Fps, A.TEST_VIDEO_FPS);
        }
        
        [Test]
        public void VideoFrameCountMatches()
        {
            Assert.AreEqual(A.VideoTools.GetTestVideo()?.Video?.FrameCount, A.TEST_VIDEO_FRAME_COUNT);
        }
        
        [Test]
        public void VideoDurationMatches()
        {
            Assert.AreEqual(A.VideoTools.GetTestVideo()?.Video?.Duration, A.TEST_VIDEO_DURATION);
        }

        [Test]
        public void VideoRenameChangesName()
        {
            const string NEW_NAME = "New Name";
            
            var tools = A.VideoTools;
            var video = tools.GetTestVideo().Video;

            tools.Rename(ref video, NEW_NAME);
            Assert.AreEqual(video.Name, NEW_NAME);
        }

        [Test]
        public void VideoRenameChangesPath()
        {
            const string NEW_NAME = "New Name";
            
            var newPath = A.TestVideoPath.Replace(A.TEST_VIDEO_NAME, NEW_NAME);
            var tools = A.VideoTools;
            var video = tools.GetTestVideo().Video;

            tools.Rename(ref video, NEW_NAME);
            Assert.AreEqual(video.Path, newPath);
        }

        [Test]
        public void ResizingVideoChangesWidth()
        {
            const int TARGET_WIDTH = 320;
            const int TARGET_HEIGHT = 180;
            
            var tools = A.VideoTools;
            var video = tools.GetTestVideo().Video;

            tools.Resize(video, TARGET_WIDTH, TARGET_HEIGHT);
            Assert.AreEqual(video.Width, TARGET_WIDTH);
        }

        [Test]
        public void ResizingVideoChangesHeight()
        {
            const int TARGET_WIDTH = 320;
            const int TARGET_HEIGHT = 180;
            
            var tools = A.VideoTools;
            var video = tools.GetTestVideo().Video;

            tools.Resize(video, TARGET_WIDTH, TARGET_HEIGHT);
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

        private static IVideoProvider VideoProvider => new UnityVideoProvider();
        private static IVideoResizer VideoResizer => new TestVideoResizer();
        private static ITimeProvider TimeProvider => new SystemTimeProvider();
        public static VideoTools VideoTools => new VideoTools(VideoProvider, VideoResizer);

        public static UnityVideo GetTestVideo(this VideoTools tools)
        {
            Video video = null;
            Texture2D thumbnail = null;
            var time = TimeProvider;
            var timeout = time.Epoch + 5.0f;

            tools.LoadVideo(TestVideoPath, (vid, thumb) => { video = vid; thumbnail = thumb; });

            while (video == null && thumbnail == null && time.Epoch < timeout)
                Thread.Sleep(10);

            return new UnityVideo(video, thumbnail);
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

        public static bool Resize(this VideoTools tools, Video video, int width, int height)
        {
            var resized = false;
            var result = false;
            var time = TimeProvider;
            var timeout = time.Epoch + 5.0f;

            tools.Resize(video, width, height, (success, err) =>
            {
                result = success;
                resized = true;
            });
            
            while (!resized && time.Epoch < timeout)
                Thread.Sleep(10);
            
            return result;
        }

        public class UnityVideo
        {
            public Video Video { get; set; }
            public Texture2D Thumbnail { get; set; }
            public UnityVideo(Video video, Texture2D thumbnail) { Video = video; Thumbnail = thumbnail; }
        }
    }
}
