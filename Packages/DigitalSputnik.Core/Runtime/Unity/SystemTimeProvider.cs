using System;

namespace DigitalSputnik
{
    public class SystemTimeProvider : ITimeProvider
    {
        public double Epoch => (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
    }
}