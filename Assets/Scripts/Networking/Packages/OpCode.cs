using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace VoyagerApp.Networking.Packages
{
    [Serializable]
    [JsonConverter(typeof(StringEnumConverter), typeof(SnakeCaseNamingStrategy))]
    public enum OpCode
    {
        Collection,
        DiscoveryRequest,
        DiscoveryResponse,
        InfoRequest,
        InfoResponse,
        BatteryRequest,
        BatteryResponse,
        NetworkStateRequest,
        NetworkStateResponse,
        SetNetworkStateRequest,
        SetNetworkStateResponse,
        SsidListRequest,
        SsidListResponse,
        ItshRequest,
        ItshResponse,
        SetItsh,
        VideoRequest,
        VideoResponse,
        SetVideo,
        FpsRequest,
        FpsResponse,
        SetFps,
        SetFrame
    }
}