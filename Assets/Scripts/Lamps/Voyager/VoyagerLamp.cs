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

        public string mode;
        public string activeSsid;
        public string activePassword;
        public string activePattern;
        public string activePatternPassword;

        public byte[] pixelData;

        VoyagerClient client;

        public override int pixels => length;

        public VoyagerLamp()
        {
            type = "Voyager";
        }

        internal override void Update(object data)
        {
            var unpacked = (VoyagerLampInfoResponse)data;

            serial = unpacked.serial;
            address = new IPAddress(unpacked.ip);

            length = unpacked.length;
            battery = unpacked.battery;

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

        public override void SetVideo(Video video)
        {
            base.SetVideo(video);

            var packet = new PacketCollection(
                new SetVideoPacket(video.frames, video.lastStartTime),
                new SetFpsPacket((int)video.fps)
            );

            NetUtils.VoyagerClient.SendPacket(this, packet);
            buffer.RecreateBuffer(video.frames);
        }

        public override void SetItshe(Itshe itshe)
        {
            base.SetItshe(itshe);

            var packet = new SetItshePacket(itshe);
            NetUtils.VoyagerClient.SendPacket(this, packet);
        }

        public void SendVideoWithItsh()
        {
            var packet = new PacketCollection(
                new SetVideoPacket(video.frames, video.lastStartTime),
                new SetItshePacket(itshe));
            NetUtils.VoyagerClient.SendPacket(this, packet);
        }

        public override void PushFrame(Color32[] colors, long frame)
        {
            base.PushFrame(colors, frame);

            byte[] data = ColorUtils.ColorsToBytes(colors);
            var packet = new SetFramePacket(frame, data);
            NetUtils.VoyagerClient.SendPacket(this, packet);
        }
    }
}