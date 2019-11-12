namespace VoyagerApp.Dmx
{
    public enum DmxFormat { Rgbt, Itsh, Unknown }

    public static class DmxFormatHelper
    {
        public static DmxFormat FromString(string format)
        {
            switch (format)
            {
                case "rgbt":
                    return DmxFormat.Rgbt;
                case "itsh":
                    return DmxFormat.Itsh;
                default:
                    return DmxFormat.Unknown;
            }
        }
    }
}