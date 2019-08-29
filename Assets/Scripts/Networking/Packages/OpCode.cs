using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using VoyagerApp.Utilities;

namespace VoyagerApp.Networking.Packages
{
    [Serializable]
    [JsonConverter(typeof(OpCodeConverter))]
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