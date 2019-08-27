﻿using System;

namespace VoyagerApp.Videos
{
    [Serializable]
    public class VideoBuffer
    {
        public long frames;
        public byte[][] framesToBuffer;

        public bool ContainsVideo => frames != 0;

        public void RecreateBuffer(long frames)
        {
            this.frames = frames;
            framesToBuffer = new byte[frames][];

            for (int f = 0; f < frames; f++)
                framesToBuffer[f] = new byte[0];
        }

        public void SetFrame(long frame, byte[] buffer)
        {
            framesToBuffer[frame] = buffer;
        }

        public bool FrameExists(long frame)
        {
            if (frame < 0 || frame >= frames) return false;
            return framesToBuffer[frame].Length != 0;
        }

        public long GetClosestIndex(long from, int area)
        {
            if (FrameExists(from))
                return from;

            long region = 1;
            while(region <= area)
            {
                if (FrameExists(from + region))
                    return from + region;
                if (FrameExists(from - region))
                    return from - region;
                region++;
            }

            return -1;
        }

        public byte[] GetFrame(long frame)
        {
            return framesToBuffer[frame];
        }
    }
}