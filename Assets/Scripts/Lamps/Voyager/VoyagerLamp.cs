using System;
using System.Net;
using UnityEngine;
using VoyagerApp.Dmx;
using VoyagerApp.Networking.Voyager;
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
        public bool passive;
        public bool charging;

        public string mode;
        public string activeSsid;
        public string activePassword;
        public string activePattern;
        public string activePatternPassword;

        public bool dmxEnabled;
        public int dmxUniverse;
        public int dmxChannel;
        public int dmxDivision;
        public DmxProtocol dmxProtocol;
        public DmxFormat dmxFormat;

        public override bool connected => base.connected && !passive;
        public override int pixels => length;
        public override string version => chipVersion == null ? "0.0" : string.Join(".", chipVersion);
        public override double lastTimestamp { get; protected set; }

        public override bool updated
        {
            get
            {
                Version lampVersion = new Version(version);
                Version softwareVersion = new Version(UpdateSettings.VoyagerAnimationVersion);
                return lampVersion.CompareTo(softwareVersion) >= 0;
            }
        }

        public double last = 0.0f;

        public VoyagerLamp()
        {
            type = "Voyager";
            itshe = Itshe.white;
        }

        internal override void Update(object data)
        {
            if (data is VoyagerLampInfoResponse info)
            {
                serial = info.serial;
                address = new IPAddress(info.ip);

                length = info.length;
                battery = info.battery;
                animVersion = info.animVersion;
                chipVersion = info.chipVersion;
                passive = info.passiveActiveMode == "1";
                //charging = info.chargingStatus

                mode = info.activeMode;
                activeSsid = info.activeSsid;
                activePassword = info.activePassword;
                activePattern = info.activePattern;
                activePatternPassword = info.activePatternPassword;
            }
            else if (data is DmxModeResponse dmx)
            {
                dmxEnabled = dmx.enabled;
                dmxUniverse = dmx.universe;
                dmxChannel = dmx.channel;
                dmxDivision = dmx.division;
                dmxProtocol = DmxProtocolHelper.FromString(dmx.protocol);
                dmxFormat = DmxFormatHelper.FromString(dmx.format);
            }

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
            if (video == null) return;

            last = TimeUtils.Epoch + NetUtils.VoyagerClient.TimeOffset;
            var start = video.lastStartTime + NetUtils.VoyagerClient.TimeOffset;
            var packet = new PacketCollection(
                new SetVideoPacket(video.frames, start),
                new SetFpsPacket(video.fps),
                new SetItshePacket(itshe),
                new SetPlayModePacket(PlaybackMode.Play)
            );
            NetUtils.VoyagerClient.KeepSendingPacket(this, "set_video", packet, VoyagerClient.PORT_SETTINGS, last);
            buffer.RecreateBuffer(video.frames);
            lastTimestamp = last;
            base.SetVideo(video);
        }

        public override void SetItshe(Itshe itshe)
        {
            base.SetItshe(itshe);

            last = TimeUtils.Epoch + NetUtils.VoyagerClient.TimeOffset;
            var packet = new PacketCollection(
                new SetVideoPacket(video.frames, video.lastStartTime),
                new SetFpsPacket(video.fps),
                new SetItshePacket(itshe)
            );
            NetUtils.VoyagerClient.KeepSendingPacket(this, "set_video", packet, VoyagerClient.PORT_SETTINGS, last);
            buffer.ClearBuffer();
            lastTimestamp = last;
        }

        public override void PushFrame(Color32[] colors, long frame)
        {
            byte[] data = ColorUtils.ColorsToBytes(colors);
            var packet = new SetFramePacket(frame, itshe, data);
            NetUtils.VoyagerClient.SendPacket(this, packet, VoyagerClient.PORT_VIDEO, last);
            base.PushFrame(colors, frame);
        }
    }
}