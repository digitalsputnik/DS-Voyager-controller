using System;

namespace DigitalSputnik.Videos
{
    public class UnityTimeProvider : ITimeProvider
    {
        public double Epoch => (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
    }
}