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
        private static LampEffectsWorker _instance;
        private void Awake() => _instance = this;

        public static void ApplyEffectToLamp(Lamp lamp, Effect effect)
        {
            if (ApplicationManager.Lamps.GetMetadata(lamp.Serial).Effect == effect)
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
            var meta = ApplicationManager.Lamps.GetMetadata(voyager.Serial);
            var offset = TimeOffset(voyager);
            var start = video.Meta.StartTime + offset;
            var time = TimeUtils.Epoch + offset;
            var framebuffer = new Rgb[video.Video.FrameCount][];

            var packet = new PacketCollection(
                new SetVideoPacket((long) video.Video.FrameCount, start),
                new SetItshePacket(new Itshe()), // TODO: Remove later!
                new SetFpsPacket(video.Video.Fps)
            );

            meta.TimeEffectApplied = time;
            meta.Rendered = false;
            meta.FrameBuffer = framebuffer;
            meta.Effect = video;

            SendPacket(voyager, packet, time);
        }

        public static void ApplyVideoFrameToVoyager(VoyagerLamp voyager, long index, Rgb[] rgb)
        {
            var meta = ApplicationManager.Lamps.GetMetadata(voyager.Serial);
            var time = meta.TimeEffectApplied;
            var data = ColorUtils.RgbArrayToBytes(rgb);
            var packet = new VideoFramePacket(index, data);

            meta.FrameBuffer[index] = rgb;
            if (meta.FrameBuffer.All(frame => frame != null))
                meta.Rendered = true;
            
            SendPacket(voyager, packet, time);
        }

        private static double TimeOffset(Lamp lamp)
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
    }
}