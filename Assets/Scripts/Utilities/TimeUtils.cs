using System;

namespace VoyagerApp.Utilities
{
    public static class TimeUtils
    {
        public static double Epoch
        {
            get => (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
        }
    }
}