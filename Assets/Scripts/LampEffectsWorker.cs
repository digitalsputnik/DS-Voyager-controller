using System;
using System.Collections.Generic;
using System.Linq;
using DigitalSputnik;
using DigitalSputnik.Colors;
using DigitalSputnik.Videos;
using DigitalSputnik.Voyager;
using DigitalSputnik.Voyager.Communication;
using Unity.Mathematics;
using UnityEngine;
using VoyagerController.Bluetooth;
using VoyagerController.Effects;
using VoyagerController.Serial;
using VoyagerController.Workspace;
using PlayMode = DigitalSputnik.Voyager.PlayMode;

namespace VoyagerController
{
    public class LampEffectsWorker : MonoBehaviour
    {
        private static LampEffectsWorker _instance;
        private void Awake() => _instance = this;

        private const double MISSING_FRAMES_REQUEST_FREQUENCY = 2.0;
        private const double MISSING_FRAMES_SENDING_FREQUENCY = 2.0;

        private readonly List<VoyagerLamp> _confirmingLamps = new List<VoyagerLamp>();
        
        private double _previousMissingFramesRequest;
        private double _previousMissingFramesSent;

        private void Start()
        {
            LampManager.Instance
                .GetClient<VoyagerNetworkClient>()
                .AddOpListener<MissingFramesResponse>(OpCode.MissingFramesResponse, OnMissingFramesResponse);
            ApplicationState.Playmode.OnChanged += GlobalPlaymodeChanged;
            ApplicationState.GlobalDimmer.OnChanged += DimmerChanged;
            WorkspaceManager.ItemAdded += WorkspaceItemAdded;
        }

        private void OnDestroy()
        {
            LampManager.Instance
                .GetClient<VoyagerNetworkClient>()?
                .RemoveOpListener<MissingFramesResponse>(OpCode.MissingFramesResponse, OnMissingFramesResponse);
            ApplicationState.Playmode.OnChanged -= GlobalPlaymodeChanged;
            ApplicationState.GlobalDimmer.OnChanged -= DimmerChanged;
            WorkspaceManager.ItemAdded -= WorkspaceItemAdded;
        }

        private void Update()
        {
            ConfirmLampFrames();
            ClearConfirmingLamps();
            RequestMissingFrames();
            ResendMissingFrames();
        }

        private void GlobalPlaymodeChanged(GlobalPlaymode value)
        {
            LampPlaymodeChanged((PlayMode) (int) value);
        }

        private void ConfirmLampFrames()
        {
            var unconfirmed = LampManager.Instance.GetLampsOfType<VoyagerLamp>().Where(l =>
            {
                var meta = Metadata.Get<LampData>(l.Serial);
                
                if (_confirmingLamps.Contains(l)) return false;
                if (!(meta.Effect is VideoEffect || meta.Effect is ImageEffect)) return false;

                return meta.TotalMissingFrames > 0;
            });

            foreach (var voyager in unconfirmed)
                StartConfirmingLamp(voyager);
        }

        private void StartConfirmingLamp(VoyagerLamp voyager)
        {
            _confirmingLamps.Add(voyager);
            DebugConsole.LogInfo($"Started confirming lamp {voyager.Serial}");
        }

        private void ClearConfirmingLamps()
        {
            var confirmed =  _confirmingLamps.Where(l =>
            {
                var meta = Metadata.Get<LampData>(l.Serial);
                if (!(meta.Effect is VideoEffect || meta.Effect is ImageEffect)) return true;
                return meta.TotalMissingFrames == 0;
            });

            foreach (var voyager in confirmed.ToList())
                EndConfirmingLamp(voyager);
        }
        
        private void EndConfirmingLamp(VoyagerLamp voyager)
        {
            _confirmingLamps.Remove(voyager);
            DebugConsole.LogInfo($"Ended confirming lamp {voyager.Serial}");
        }

        private void RequestMissingFrames()
        {
            if (TimeUtils.Epoch < _previousMissingFramesRequest + MISSING_FRAMES_REQUEST_FREQUENCY) return;
            
            _confirmingLamps.ForEach(RequestMissingFrames);
            _previousMissingFramesRequest = TimeUtils.Epoch;
        }

        private static void RequestMissingFrames(VoyagerLamp voyager)
        {
            var videoTime = Metadata.Get<LampData>(voyager.Serial).TimeEffectApplied;
            var packet = new MissingFramesRequest(videoTime);
            var time = TimeUtils.Epoch + TimeOffset(voyager);
            GetLampClient(voyager)?.SendSettingsPacket(voyager, packet, time);
        }
        
        private static void OnMissingFramesResponse(VoyagerLamp voyager, MissingFramesResponse response)
        {
            var meta = Metadata.Get<LampData>(voyager.Serial);

            if (Math.Abs(meta.TimeEffectApplied - response.VideoId) > 0.0001) return;
            
            meta.TotalMissingFrames = response.TotalMissing;
            
            for (var i = 0; i < meta.ConfirmedFrames.Length; i++)
                meta.ConfirmedFrames[i] = true;
            foreach (var index in response.Indices)
                meta.ConfirmedFrames[index] = false;
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
            var meta = Metadata.Get<LampData>(voyager.Serial);
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
            var meta = Metadata.Get<LampData>(voyager.Serial);
            var time = TimeUtils.Epoch;
            var start = time;
            var framebuffer = new Rgb[video.Video.FrameCount][];
            var confirmed = new bool[video.Video.FrameCount];
            var total = (long) video.Video.FrameCount;
            var client = GetLampClient(voyager);

            if (client != null)
            {
                time = client.StartVideo(voyager, (long) video.Video.FrameCount, start);
                client.SetItshe(voyager, meta.Itshe);
                client.SetFps(voyager, video.Video.Fps);
            }

            for (var i = 0; i < framebuffer.Length; i++) framebuffer[i] = null;

            meta.TimeEffectApplied = time;
            meta.VideoStartTime = start;
            meta.Rendered = false;
            meta.FrameBuffer = framebuffer;
            meta.Effect = video;
            meta.ConfirmedFrames = confirmed;
            meta.TotalMissingFrames = total;
        }
        
        private static void ApplyImageToVoyager(VoyagerLamp voyager, ImageEffect image)
        {
            var meta = Metadata.Get<LampData>(voyager.Serial);
            var time = TimeUtils.Epoch;
            var offset = TimeOffset(voyager);
            var start = time + offset;
            var framebuffer = new Rgb[1][];
            var confirmed = new bool[1];
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
            meta.TotalMissingFrames = 1;
        }

        public static void ApplyItsheToVoyager(VoyagerLamp voyager, Itshe itshe)
        {
            var meta = Metadata.Get<LampData>(voyager.Serial);
            GetLampClient(voyager)?.SetItshe(voyager, itshe);
            meta.Itshe = itshe.Clone();
        }

        public static void ApplyVideoFrameToVoyager(VoyagerLamp voyager, long index, Rgb[] rgb)
        {
            var meta = Metadata.Get<LampData>(voyager.Serial);
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
            var meta = Metadata.Get<LampData>(voyager.Serial);
            var start = meta.TimeEffectApplied;
            var time = TimeUtils.Epoch + TimeOffset(voyager) + delay;
            GetLampClient(voyager)?.SendStreamFrame(voyager, start, time, rgb);
            meta.PreviousStreamFrame = rgb;
        }

        public static double TimeOffset(Lamp lamp) => GetLampClient(lamp)?.TimeOffset ?? 0.0;

        public static long GetCurrentFrameOfVideo(VoyagerLamp voyager, Video video, double addSeconds = 0)
        {
            var meta = Metadata.Get<LampData>(voyager.Serial);
            double since;
            long frames;
            
            switch (ApplicationState.Playmode.Value)
            {
                case GlobalPlaymode.Play:
                    since = TimeUtils.Epoch - meta.VideoStartTime + addSeconds;
                    if (video == null) return -1;
                    frames = (long) math.ceil(since * video.Fps);
                    while (frames < 0) frames += (long) video.FrameCount;
                    return frames % (long)video.FrameCount;
                
                case GlobalPlaymode.Pause:
                    since = ApplicationState.PlaymodePausedSince.Value - meta.VideoStartTime + addSeconds;
                    if (video == null) return -1;
                    frames = (long) math.ceil(since * video.Fps);
                    while (frames < 0) frames += (long)video.FrameCount;
                    return frames % (long)video.FrameCount;
                    
                case GlobalPlaymode.Stop:
                    return 0;
                default:
                    return -1;
            }
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

        public static void ModifyVideoStartTime()
        {
            if (ApplicationState.PlaymodePausedSince.Value > 0.0)
            {
                var pauseTime = TimeUtils.Epoch - ApplicationState.PlaymodePausedSince.Value;
                foreach (var voyager in WorkspaceManager.GetItems<VoyagerItem>().Select(v => v.LampHandle))
                    Metadata.Get<LampData>(voyager.Serial).VideoStartTime += pauseTime;
            }
            else
            {
                foreach (var voyager in WorkspaceManager.GetItems<VoyagerItem>().Select(v => v.LampHandle))
                    Metadata.Get<LampData>(voyager.Serial).VideoStartTime = TimeUtils.Epoch;
            }
        }

        private static void LampPlaymodeChanged(PlayMode mode)
        {
            foreach (var voyager in WorkspaceManager.GetItems<VoyagerItem>().Select(v => v.LampHandle))
            {
                var effect = Metadata.Get<LampData>(voyager.Serial).Effect;
                
                if (effect is VideoEffect)
                {
                    var startTime = Metadata.Get<LampData>(voyager.Serial).VideoStartTime;
                    var handleAt = TimeUtils.Epoch + TimeOffset(voyager) + ApplicationSettings.PLAYBACK_OFFSET;
                    voyager.SetPlayMode(mode, startTime, handleAt);
                }
            }
        }

        private static void DimmerChanged(float value)
        {
            foreach (var voyager in WorkspaceManager.GetItems<VoyagerItem>().Select(v => v.LampHandle))
                voyager.SetGlobalIntensity(value);
        }

        private static void WorkspaceItemAdded(WorkspaceItem item)
        {
            if (item is VoyagerItem voyager)
            {
                if (voyager.LampHandle.Endpoint != null)
                    voyager.LampHandle?.SetGlobalIntensity(ApplicationState.GlobalDimmer.Value);
            }
        }
        
        private void ResendMissingFrames()
        {
            if (TimeUtils.Epoch < _previousMissingFramesSent + MISSING_FRAMES_SENDING_FREQUENCY) return;
            
            foreach (var voyager in _confirmingLamps)
            {
                var meta = Metadata.Get<LampData>(voyager.Serial);

                if (meta.Effect is VideoEffect)
                {
                    var time = meta.TimeEffectApplied;
                    var client = GetLampClient(voyager);
                    var indices = new List<int>();
                
                    for (var i = 0; i < meta.ConfirmedFrames.Length; i++)
                    {
                        if (!meta.ConfirmedFrames[i] && meta.FrameBuffer[i] != null)
                            indices.Add(i);
                    }

                    foreach (var index in indices)
                    {
                        var frame = meta.FrameBuffer[index];
                        client?.SendVideoFrame(voyager, index, time, frame);
                    }
                }
            }

            _previousMissingFramesSent = TimeUtils.Epoch;
        }
    }
}