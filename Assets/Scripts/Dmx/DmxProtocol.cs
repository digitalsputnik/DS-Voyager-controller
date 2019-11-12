namespace VoyagerApp.Dmx
{
    public enum DmxProtocol { ArtNet, sACN, Unknown }

    public static class DmxProtocolHelper
    {
        public static DmxProtocol FromString(string protocol)
        {
            switch (protocol)
            {
                case "ArtNet":
                    return DmxProtocol.ArtNet;
                case "sACN":
                    return DmxProtocol.sACN;
                default:
                    return DmxProtocol.Unknown;
            }
        }
    }
}