using System;
using System.Collections.Generic;
using System.Linq;
using DigitalSputnik;
using DigitalSputnik.Colors;
using DigitalSputnik.Voyager;
using DigitalSputnik.Voyager.Communication;
using UnityEngine;
using VoyagerController.Effects;

namespace VoyagerController
{
    public class LampEffectsWorker : MonoBehaviour
    {
        private const double MISSING_FRAMES_REQUEST_FREQUENCY = 2.0;

        private readonly List<VoyagerLamp> _confirmingLamps = new List<VoyagerLamp>();
        private double _previousMissingFramesRequest;

        private void Start()
        {
            VoyagerClient.Instance.AddOpListener<MissingFramesResponse>(OpCode.MissingFramesResponse, OnMissingFramesResponse);
        }

        private void OnDestroy()
        {
            VoyagerClient.Instance?.RemoveOpListener<MissingFramesResponse>(OpCode.MissingFramesResponse, OnMissingFramesResponse);
        }

        private void Update()
        {
            ConfirmLampFrames();
            ClearConfirmingLamps();
            RequestMissingFrames();
        }

        private void ConfirmLampFrames()
        {
            var unconfirmed = LampManager.Instance.GetLampsOfType<VoyagerLamp>().Where(l =>
            {
                var meta = Metadata.Get(l.Serial);
                
                if (_confirmingLamps.Contains(l)) return false;
                if (!(meta.Effect is VideoEffect)) return false;

                return meta.ConfirmedFrames
                    .Where((c, index) => meta.FrameBuffer[index] != null && c == false)
                    .Any();
            });

            foreach (var voyager in unconfirmed)
                StartConfirmingLamp(voyager);
        }

        private void StartConfirmingLamp(VoyagerLamp voyager)
        {
            _confirmingLamps.Add(voyager);
            Debugger.LogInfo($"Started confirming lamp {voyager.Serial}");
        }

        private void ClearConfirmingLamps()
        {
            var confirmed =  _confirmingLamps.Where(l =>
            {
                var meta = Metadata.Get(l.Serial);
                if (!(meta.Effect is VideoEffect)) return true;
                return meta.Rendered && meta.ConfirmedFrames.All(c => c);
            });

            foreach (var voyager in confirmed.ToList())
                EndConfirmingLamp(voyager);
        }
        
        private void EndConfirmingLamp(VoyagerLamp voyager)
        {
            _confirmingLamps.Remove(voyager);
            Debugger.LogInfo($"Ended confirming lamp {voyager.Serial}");
        }


        private void RequestMissingFrames()
        {
            if (TimeUtils.Epoch < _previousMissingFramesRequest + MISSING_FRAMES_REQUEST_FREQUENCY) return;
            
            _confirmingLamps.ForEach(RequestMissingFrames);
            _previousMissingFramesRequest = TimeUtils.Epoch;
        }

        private static void RequestMissingFrames(VoyagerLamp voyager)
        {
            var videoTime = Metadata.Get(voyager.Serial).TimeEffectApplied;
            var packet = new MissingFramesRequest(videoTime);
            var time = TimeUtils.Epoch + TimeOffset(voyager);
            SendPacket(voyager, packet, time);
        }

        public static void ApplyEffectToLamp(Lamp lamp, Effect effect)
        {
            if (Metadata.Get(lamp.Serial).Effect == effect)
                return;

            switch (lamp)
            {
                case VoyagerLamp voyager:
                    if (effect is VideoEffect video)
                        ApplyVideoToVoyager(voyager, video);
                    break;
            }
        }

        private static void ApplyVideoToVoyager(VoyagerLamp voyager, VideoEffect video)
        {
            var meta = Metadata.Get(voyager.Serial);
            var offset = TimeOffset(voyager);
            var start = video.Meta.StartTime + offset;
            var time = TimeUtils.Epoch + offset;
            var framebuffer = new Rgb[video.Video.FrameCount][];
            var confirmed = new bool[video.Video.FrameCount];

            var packet = new PacketCollection(
                new SetVideoPacket((long) video.Video.FrameCount, start),
                new SetItshePacket(new Itshe()), // TODO: Remove later!
                new SetFpsPacket(video.Video.Fps)
            );

            for (var i = 0; i < framebuffer.Length; i++) framebuffer[i] = null;

            meta.TimeEffectApplied = time;
            meta.Rendered = false;
            meta.FrameBuffer = framebuffer;
            meta.Effect = video;
            meta.ConfirmedFrames = confirmed;

            SendPacket(voyager, packet, time);
        }

        public static void ApplyVideoFrameToVoyager(VoyagerLamp voyager, long index, Rgb[] rgb)
        {
            var meta = Metadata.Get(voyager.Serial);
            var time = meta.TimeEffectApplied;
            var data = ColorUtils.RgbArrayToBytes(rgb);
            var packet = new VideoFramePacket(index, data);

            meta.FrameBuffer[index] = rgb;
            if (meta.FrameBuffer.All(frame => frame != null))
                meta.Rendered = true;
            
            SendPacket(voyager, packet, time);
        }

        public static double TimeOffset(Lamp lamp)
        {
            switch (lamp.Endpoint)
            {
                case LampNetworkEndPoint _:
                    return VoyagerClient.Instance.TimeOffset;
            }

            return 0.0;
        }

        private static void SendPacket(VoyagerLamp voyager, Packet packet, double time)
        {
            switch (voyager.Endpoint)
            {
                case LampNetworkEndPoint _:
                    if (packet is VideoFramePacket)
                        VoyagerClient.Instance.SendFramePacket(voyager, packet, time);
                    else
                        VoyagerClient.Instance.SendSettingsPacket(voyager, packet, time);
                    break;
            }
        }
        
        private static void OnMissingFramesResponse(VoyagerLamp voyager, MissingFramesResponse response)
        {
            var meta = Metadata.Get(voyager.Serial);

            if (Math.Abs(meta.TimeEffectApplied - response.VideoId) > 0.0001) return;
            
            for (var i = 0; i < meta.ConfirmedFrames.Length; i++)
                meta.ConfirmedFrames[i] = true;
            foreach (var index in response.Indices)
                meta.ConfirmedFrames[index] = false;
        }
    }
}