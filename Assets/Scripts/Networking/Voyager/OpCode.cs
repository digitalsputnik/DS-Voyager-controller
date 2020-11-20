using System;
using Newtonsoft.Json;
using VoyagerApp.Utilities;

namespace VoyagerApp.Networking.Voyager
{
    [Serializable]
    [JsonConverter(typeof(OpCodeConverter))]
    public enum OpCode
    {
        PollRequest,
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
        SetItshe,
        VideoRequest,
        VideoResponse,
        SetVideo,
        FpsRequest,
        FpsResponse,
        SetFps,
        SetFrame,
        SetEffect,
        MissingFramesRequest,
        MissingFramesResponse,
        SetPlayMode,
        PixelOverride,
        SetDmxMode,
        DmxModeRequest,
        DmxModeResponse,
        SetGlobalIntensity,
        SetStream,
        StreamFrame,
        ActivateVideoTrigger,

        // BLE
        GetSerial,
        GetChipVersion,
        PollReply
    }
}