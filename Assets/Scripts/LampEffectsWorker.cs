using DigitalSputnik;
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

            ApplicationManager.Lamps.GetMetadata(lamp.Serial).Effect = effect;

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
            var offset = TimeOffset(voyager);
            var start = video.Meta.StartTime + offset;
            var time = TimeUtils.Epoch + offset;

            var packet = new PacketCollection(
                new SetVideoPacket((long)video.Video.FrameCount, start),
                new SetFpsPacket(video.Video.Fps)
            );

            ApplicationManager.Lamps.GetMetadata(voyager.Serial).TimeEffectApplied = time;

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
                case LampNetworkEndPoint networkEndPoint:
                    VoyagerClient.Instance.SendSettingsPacket(voyager, packet, time);
                    break;
            }
        }
    }
}