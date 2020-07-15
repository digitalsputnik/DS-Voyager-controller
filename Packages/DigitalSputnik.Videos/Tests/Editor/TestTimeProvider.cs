using System;

namespace DigitalSputnik.Videos.Tests
{
    internal class TestTimeProvider : ITimeProvider
    {
        public double Epoch => (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
    }
}