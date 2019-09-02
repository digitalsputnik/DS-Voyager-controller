﻿using Newtonsoft.Json;

namespace VoyagerApp.Networking.Packages.Voyager
{
    public class VideoResponsePacket : Packet
    {
        [JsonProperty("frame_count")]
        public long frameCount;
        [JsonProperty("start_time")]
        public double startTime;
        [JsonProperty("frames")]
        public byte[][] frames;

        public VideoResponsePacket() : base(OpCode.VideoResponse) { }
    }
}