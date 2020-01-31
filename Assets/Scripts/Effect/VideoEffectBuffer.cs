namespace VoyagerApp.Effects
{
    public struct VideoEffectBuffer
    {
        public long count;
        public byte[][] frames;

        public bool contains => count > 0 && frames.Length > 0;
        public bool rendered => count == existing;

        public long existing
        {
            get
            {
                long c = 0;
                for (int i = 0; i < count; i++)
                    if (FrameExists(i)) c++;
                return c;
            }
        }

        public byte[] GetFrame(long frame) => frames[frame];

        public bool InBounds(long frame) => frame >= 0 && frame < count;

        public void Setup(long count)
        {
            this.count = count;
            frames = new byte[count][];
            Clear();
        }

        public bool FrameExists(long frame)
        {
            if (!InBounds(frame)) return false;
            return frames[frame].Length != 0;
        }

        public void SetFrame(long frame, byte[] buffer)
        {
            if (!InBounds(frame)) return;
            frames[frame] = buffer;
        }

        public long GetClosestIndex(long frame, int range)
        {
            if (FrameExists(frame))
                return frame;

            for (int r = 1; r < range; r++)
            {
                if (FrameExists(frame + r)) return frame + r;
                if (FrameExists(frame - r)) return frame - r;
            }

            return -1;
        }

        public void Clear()
        {
            for (int f = 0; f < count; f++)
                frames[f] = new byte[0];
        }
    }
}