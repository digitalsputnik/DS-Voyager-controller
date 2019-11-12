using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;
using VoyagerApp.Lamps;
using VoyagerApp.Lamps.Voyager;
using VoyagerApp.Networking.Voyager;
using VoyagerApp.Utilities;

namespace VoyagerApp.Videos
{
    public class ConfirmPixelsState : RenderState
    {
        const double requestFrequency = 0.2f;

        double lastRequestTime;
        Dictionary<VoyagerLamp, long[]> missingFrames = new Dictionary<VoyagerLamp, long[]>();

        public ConfirmPixelsState()
        {
            NetUtils.VoyagerClient.onReceived += OnReceivedFromLamp;
        }

        void OnReceivedFromLamp(object sender, byte[] data)
        {
            var packet = Packet.Deserialize<MissingFramesResponsePacket>(data);
            if (packet != null && packet.op == OpCode.MissingFramesResponse)
            {
                var address = ((IPEndPoint)sender).Address;
                var lamp = (VoyagerLamp)LampManager.instance.GetLampWithAddress(address);

                if (packet.indices != null)
                {
                    Debug.Log(lamp.serial + " - " + string.Join(", ", packet.indices.Length));
                    missingFrames[lamp] = packet.indices.Where(lamp.buffer.FrameExists).ToArray();
                }
            }
        }

        public override RenderState Update()
        {
            foreach (var lamp in missingFrames.Keys.ToArray())
            {
                if (!WorkspaceUtils.Lamps.Contains(lamp) || !lamp.connected || lamp.dmxEnabled)
                    missingFrames.Remove(lamp);
            }

            if ((TimeUtils.Epoch - lastRequestTime) > requestFrequency)
            {
                SendMissingFramesRequestToLamps();
                lastRequestTime = TimeUtils.Epoch;
            }

            if (missingFrames.Count == WorkspaceUtils.VoyagerLamps.Where(l => l.connected && !l.dmxEnabled).Count())
            {
                if (missingFrames.All(f => f.Value.Length == 0))
                {
                    NetUtils.VoyagerClient.onReceived -= OnReceivedFromLamp;
                    return new DoneState();
                }
            }

            SendMissingFramesToLamps();

            VideoRenderer.UpdateProgress(Progress);

            return this;
        }

        public float Progress
        {
            get
            {
                long all = WorkspaceUtils.Lamps.Sum(l => l.buffer.frames);
                long done = WorkspaceUtils.Lamps.Sum(l => l.buffer.ExistingFramesCount);
                return (float)done / all;
            }
        }

        void SendMissingFramesRequestToLamps()
        {
            var packet = new MissingFramesRequestPacket();
            var lamps = WorkspaceUtils.Lamps.Where(l => l.connected).ToArray();

            foreach (var lamp in lamps)
                NetUtils.VoyagerClient.SendPacket(lamp, packet, VoyagerClient.PORT_SETTINGS);
        }

        void SendMissingFramesToLamps()
        {
            foreach (var lamp in missingFrames.Keys)
            {
                double time = lamp.lastTimestamp;
                foreach (var index in missingFrames[lamp])
                {
                    var frame = lamp.buffer.GetFrame(index);
                    var packet = new SetFramePacket(index, lamp.itshe, frame);
                    NetUtils.VoyagerClient.SendPacket(lamp, packet, VoyagerClient.PORT_VIDEO, time);
                }
            }
        }

        public override void OnCancel()
        {
            NetUtils.VoyagerClient.onReceived -= OnReceivedFromLamp;
        }

        public override void HandleEvent(VideoRenderEvent type) { }
    }
}
