using System;
using System.Net;
using UnityEngine;
using VoyagerApp.Networking;
using VoyagerApp.Networking.Packages;
using VoyagerApp.Networking.Packages.Voyager;
using VoyagerApp.Utilities;
using VoyagerApp.Videos;
using VoyagerApp.Workspace;
using VoyagerApp.Workspace.Views;

namespace VoyagerApp.Lamps.Voyager
{
    [Serializable]
    public class VoyagerLamp : Lamp
    {
        public int length;
        public int battery;
        public int[] animVersion;
        public int[] chipVersion;

        public string mode;
        public string activeSsid;
        public string activePassword;
        public string activePattern;
        public string activePatternPassword;

        public byte[] pixelData;

        VoyagerClient client;

        public override int pixels => length;
        public override string version => chipVersion == null ? "0.0" : string.Join(".", chipVersion);
        public override bool updated => version == UpdateSettings.VoyagerAnimationVersion;
        public override double lastTimestamp { get; protected set; }

        public VoyagerLamp()
        {
            type = "Voyager";
            itshe = Itshe.white;
        }

        internal override void Update(object data)
        {
            var unpacked = (VoyagerLampInfoResponse)data;

            serial = unpacked.serial;
            address = new IPAddress(unpacked.ip);

            length = unpacked.length;
            battery = unpacked.battery;
            animVersion = unpacked.animVersion;
            chipVersion = unpacked.chipVersion;

            mode = unpacked.activeMode;
            activeSsid = unpacked.activeSsid;
            activePassword = unpacked.activePassword;
            activePattern = unpacked.activePattern;
            activePatternPassword = unpacked.activePatternPassword;

            base.Update(data);
        }

        public override LampItemView AddToWorkspace()
        {
            return WorkspaceManager.instance.InstantiateItem<VoyagerItemView>(this);
        }

        public override LampItemView AddToWorkspace(Vector2 position)
        {
            return WorkspaceManager.instance.InstantiateItem<VoyagerItemView>(this, position);
        }

        public override LampItemView AddToWorkspace(Vector2 position, float scale)
        {
            return WorkspaceManager.instance.InstantiateItem<VoyagerItemView>(this, position, scale);
        }

        public override LampItemView AddToWorkspace(Vector2 position, float scale, float rotation)
        {
            return WorkspaceManager.instance.InstantiateItem<VoyagerItemView>(this, position, scale, rotation);
        }

        public double last = 0.0f;
        bool skip = false;

        public override void SetVideo(Video video)
        {
            base.SetVideo(video);
            last = TimeUtils.Epoch + NetUtils.VoyagerClient.TimeOffset;
            var start = video.lastStartTime + NetUtils.VoyagerClient.TimeOffset;
            var packet = new PacketCollection(
                new SetVideoPacket(video.frames, start),
                new SetFpsPacket((int)video.fps),
                new SetItshePacket(itshe)
            );
            NetUtils.VoyagerClient.SendPacket(this, packet, last);
            buffer.RecreateBuffer(video.frames);
            lastTimestamp = last;
            skip = true;
        }

        public override void SetItshe(Itshe itshe)
        {
            base.SetItshe(itshe);

            var packet = new SetItshePacket(itshe);
            NetUtils.VoyagerClient.SendPacket(this, packet);
        }

        public override void PushFrame(Color32[] colors, long frame)
        {
            if (skip)
            {
                skip = false;
                return;
            }

            byte[] data = ColorUtils.ColorsToBytes(colors);
            PushFrame(data, frame);
        }
        
        public override void PushFrame(byte[] colors, long frame)
        {
            if (skip)
            {
                skip = false;
                return;
            }

            if (!buffer.FrameExists(frame))
            {
                base.PushFrame(colors, frame);
                var packet = new SetFramePacket(frame, colors);
                NetUtils.VoyagerClient.SendPacketToVideoPort(this, packet, last);
            }
        }
    }
}