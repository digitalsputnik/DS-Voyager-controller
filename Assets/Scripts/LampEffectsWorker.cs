using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using DigitalSputnik;
using DigitalSputnik.Colors;
using DigitalSputnik.Videos;
using DigitalSputnik.Voyager;
using DigitalSputnik.Voyager.Communication;
using UnityEngine;
using VoyagerController.Bluetooth;
using VoyagerController.Effects;
using VoyagerController.Serial;

namespace VoyagerController
{
    public class LampEffectsWorker : MonoBehaviour
    {
        private static LampEffectsWorker _instance;
        private void Awake() => _instance = this;

        private const double MISSING_FRAMES_REQUEST_FREQUENCY = 2.0;

        private readonly List<VoyagerLamp> _confirmingLamps = new List<VoyagerLamp>();
        private double _previousMissingFramesRequest;

        private void Start()
        {
            LampManager.Instance
                .GetClient<VoyagerNetworkClient>()
                .AddOpListener<MissingFramesResponse>(OpCode.MissingFramesResponse, OnMissingFramesResponse);
        }

        private void OnDestroy()
        {
            LampManager.Instance
                .GetClient<VoyagerNetworkClient>()?
                .RemoveOpListener<MissingFramesResponse>(OpCode.MissingFramesResponse, OnMissingFramesResponse);
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
                if (!(meta.Effect is VideoEffect || meta.Effect is ImageEffect)) return false;

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
                if (!(meta.Effect is VideoEffect || meta.Effect is ImageEffect)) return true;
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

        private void RequestMissingFrames(VoyagerLamp voyager)
        {
            var videoTime = Metadata.Get(voyager.Serial).TimeEffectApplied;
            var packet = new MissingFramesRequest(videoTime);
            var time = TimeUtils.Epoch + TimeOffset(voyager);
            GetLampClient(voyager)?.SendSettingsPacket(voyager, packet, time);
        }

        public static void ApplyEffectToLamp(Lamp lamp, Effect effect)
        {
            switch (lamp)
            {
                case VoyagerLamp voyager:
                    switch (effect)
                    {
                        case VideoEffect video:
                            ApplyVideoToVoyager(voyager, video);
                            break;
                        case SyphonEffect _:
                        case SpoutEffect _:
                            ApplyStreamToVoyager(voyager, effect);
                            break;
                        case ImageEffect image:
                            ApplyImageToVoyager(voyager, image);
                            break;
                    }
                    break;
            }
        }

        private static void ApplyStreamToVoyager(VoyagerLamp voyager, Effect effect)
        {
            var meta = Metadata.Get(voyager.Serial);
            var time = TimeUtils.Epoch;
            var client = GetLampClient(voyager);

            if (client != null)
            {
                time = GetLampClient(voyager).StartStream(voyager);
                client.SetItshe(voyager, meta.Itshe);
            }
            
            meta.TimeEffectApplied = time;
            meta.Effect = effect;
        }
        
        private static void ApplyVideoToVoyager(VoyagerLamp voyager, VideoEffect video)
        {
            var meta = Metadata.Get(voyager.Serial);
            var offset = TimeOffset(voyager);
            var start = video.Meta.StartTime + offset;
            var framebuffer = new Rgb[video.Video.FrameCount][];
            var confirmed = new bool[video.Video.FrameCount];
            var time = TimeUtils.Epoch;
            var client = GetLampClient(voyager);

            if (client != null)
            {
                time = client.StartVideo(voyager, (long) video.Video.FrameCount, start);
                client.SetItshe(voyager, meta.Itshe);   
            }

            for (var i = 0; i < framebuffer.Length; i++) framebuffer[i] = null;

            meta.TimeEffectApplied = time;
            meta.VideoStartTime = start;
            meta.Rendered = false;
            meta.FrameBuffer = framebuffer;
            meta.Effect = video;
            meta.ConfirmedFrames = confirmed;
        }
        
        private static void ApplyImageToVoyager(VoyagerLamp voyager, ImageEffect image)
        {
            var meta = Metadata.Get(voyager.Serial);
            var offset = TimeOffset(voyager);
            var start = image.Meta.StartTime + offset;
            var framebuffer = new Rgb[1][];
            var confirmed = new bool[1];
            var time = TimeUtils.Epoch;
            var client = GetLampClient(voyager);

            if (client != null)
            {
                time = client.StartVideo(voyager, 1, start);
                client.SetItshe(voyager, meta.Itshe);   
            }

            framebuffer[0] = null;

            meta.TimeEffectApplied = time;
            meta.VideoStartTime = start;
            meta.Rendered = false;
            meta.FrameBuffer = framebuffer;
            meta.Effect = image;
            meta.ConfirmedFrames = confirmed;
        }

        public static void ApplyItsheToVoyager(VoyagerLamp voyager, Itshe itshe)
        {
            var meta = Metadata.Get(voyager.Serial);
            GetLampClient(voyager)?.SetItshe(voyager, meta.Itshe);
            Debug.Log(GetLampClient(voyager));
            meta.Itshe = itshe;
        }

        public static void ApplyVideoFrameToVoyager(VoyagerLamp voyager, long index, Rgb[] rgb)
        {
            var meta = Metadata.Get(voyager.Serial);
            var time = meta.TimeEffectApplied;
            
            GetLampClient(voyager)?.SendVideoFrame(voyager, index, time, rgb);
            
            meta.FrameBuffer[index] = rgb;
            if (meta.FrameBuffer.All(frame => frame != null))
                meta.Rendered = true;

            if (voyager.Endpoint is BluetoothEndPoint)
                meta.ConfirmedFrames[index] = true;
        }

        public static void ApplyStreamFrameToVoyager(VoyagerLamp voyager, Rgb[] rgb, double delay)
        {
            var meta = Metadata.Get(voyager.Serial);
            var start = meta.TimeEffectApplied;
            var time = TimeUtils.Epoch + TimeOffset(voyager) + delay;
            GetLampClient(voyager)?.SendStreamFrame(voyager, start, time, rgb);
            meta.PreviousStreamFrame = rgb;
        }

        public static double TimeOffset(Lamp lamp) => GetLampClient(lamp)?.TimeOffset ?? 0.0;

        private static void OnMissingFramesResponse(VoyagerLamp voyager, MissingFramesResponse response)
        {
            var meta = Metadata.Get(voyager.Serial);

            if (Math.Abs(meta.TimeEffectApplied - response.VideoId) > 0.0001) return;
            
            for (var i = 0; i < meta.ConfirmedFrames.Length; i++)
                meta.ConfirmedFrames[i] = true;
            foreach (var index in response.Indices)
                meta.ConfirmedFrames[index] = false;
        }

        public static long GetCurrentFrameOfVideo(VoyagerLamp voyager, Video video, long add = 0)
        {
            var meta = Metadata.Get(voyager.Serial);
            var since = TimeUtils.Epoch - meta.VideoStartTime + TimeOffset(voyager);

            if (video == null) return 0;
            
            var frames = (long)(since * video.Fps) + add;
            while (frames < 0) frames += (long)video.FrameCount;
            return frames % (long)video.FrameCount;

        }

        private static VoyagerClient GetLampClient(Lamp lamp)
        {
            switch (lamp.Endpoint)
            {
                case LampNetworkEndPoint _:
                    return LampManager.Instance.GetClient<VoyagerNetworkClient>();
                case BluetoothEndPoint _:
                    return LampManager.Instance.GetClient<VoyagerBluetoothClient>();
                case SerialEndPoint _:
                    return LampManager.Instance.GetClient<VoyagerSerialClient>();
            }
            return null;
        }
    }
}