using System;
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
        const double _requestFrequency = 0.1f;

        double _lastRequestTime;
        Dictionary<VoyagerLamp, long[]> _missingFrames = new Dictionary<VoyagerLamp, long[]>();


        private bool _abort = false;
        private double _startTime;
        private bool _rechecked;

        public ConfirmPixelsState()
        {
            _startTime = TimeUtils.Epoch;
            NetUtils.VoyagerClient.onReceived += OnReceivedFromLamp;
        }

        void OnReceivedFromLamp(object sender, byte[] data)
        {
            var packet = Packet.Deserialize<MissingFramesResponsePacket>(data);
            if (packet == null || packet.op != OpCode.MissingFramesResponse) return;
            
            var address = ((IPEndPoint)sender).Address;
            var lamp = (VoyagerLamp)LampManager.instance.GetLampWithAddress(address);

            if (Math.Abs(packet.videoTimestamp - lamp.lastTimestamp) > 0.00001) return;

            if (packet.indices.Length > 0)
                Debug.Log(lamp.serial + " - " + string.Join(", ", packet.indices));

            if (packet.indices.Length > lamp.buffer.count / 2)
            {
                lamp.buffer.Clear();
                _abort = true;
                return;
            }

            _missingFrames[lamp] = packet.indices.Where(lamp.buffer.FrameExists).ToArray();
        }

        public override RenderState Update()
        {
            if (_abort)
            {
                NetUtils.VoyagerClient.onReceived -= OnReceivedFromLamp;
                return new PrepereQueueState();
            }
            
            foreach (var lamp in _missingFrames.Keys.ToArray())
            {
                if (!WorkspaceUtils.Lamps.Contains(lamp) || !lamp.connected || lamp.dmxEnabled)
                    _missingFrames.Remove(lamp);
            }

            if (TimeUtils.Epoch - _lastRequestTime > _requestFrequency)
            {
                SendMissingFramesRequestToLamps();
                _lastRequestTime = TimeUtils.Epoch;
            }

            if (_missingFrames.Count == WorkspaceUtils.VoyagerLamps.Count(l => l.connected && !l.dmxEnabled))
            {
                if (_missingFrames.All(f => f.Value.Length == 0))
                {
                    if (!_rechecked)
                    {
                        _startTime = TimeUtils.Epoch;
                        _missingFrames.Clear();
                        _rechecked = true;
                    }
                    else
                    {
                        NetUtils.VoyagerClient.onReceived -= OnReceivedFromLamp;
                        return new DoneState();   
                    }
                }
            }

            SendMissingFramesToLamps();

            if (TimeUtils.Epoch - _startTime > 0.5)
                VideoRenderer.UpdateProgress(Progress);

            return this;
        }

        public float Progress
        {
            get
            {
                var all = WorkspaceUtils.Lamps.Sum(l => l.buffer.count);
                long missing = 0;
                
                foreach (var lamp in _missingFrames.Keys)
                    missing += _missingFrames[lamp].Count();

                return (float)(all - missing) / all;
            }
        }

        void SendMissingFramesRequestToLamps()
        {
            var lamps = WorkspaceUtils.Lamps.Where(l => l.connected).ToArray();

            foreach (var lamp in lamps)
            {
                var packet = new MissingFramesRequestPacket(lamp.lastTimestamp);
                NetUtils.VoyagerClient.SendPacket(lamp, packet, VoyagerClient.PORT_SETTINGS);
            }
        }

        void SendMissingFramesToLamps()
        {
            foreach (var lamp in _missingFrames.Keys)
            {
                var time = lamp.lastTimestamp;
                foreach (var index in _missingFrames[lamp])
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
