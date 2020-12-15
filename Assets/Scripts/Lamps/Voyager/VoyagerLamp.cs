using System;
using System.Net;
using UnityEngine;
using VoyagerApp.Dmx;
using VoyagerApp.Effects;
using VoyagerApp.Networking.Voyager;
using VoyagerApp.Utilities;
using VoyagerApp.Workspace;
using VoyagerApp.Workspace.Views;

namespace VoyagerApp.Lamps.Voyager
{
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
        public bool dmxPollReceived;

        public byte[] prevStream;

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
            mapping = EffectMapping.Default;
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

                var chargingCurrent = BitConverter.ToInt16(
                    new[]
                    {
                        (byte)info.chargingStatus[1],
                        (byte)info.chargingStatus[0]
                    }, 0);

                charging = chargingCurrent > 0;

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
                dmxPollReceived = true;
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

        public override void SetEffect(Effect effect)
        {
            if (effect == null) return;

            last = TimeUtils.Epoch + NetUtils.VoyagerClient.TimeOffset;

            if (effect is Video video)
            {
                var start = video.startTime + NetUtils.VoyagerClient.TimeOffset;

                var packet = new PacketCollection(
                    new SetVideoPacket(video.frames, start),
                    new SetFpsPacket(video.fps),
                    new SetItshePacket(itshe)
                );

                NetUtils.VoyagerClient.KeepSendingPacket(
                    this,
                    "set_effect",
                    packet,
                    VoyagerClient.PORT_SETTINGS,
                    last);

                NetUtils.VoyagerClient.KeepSendingPacket(
                    this,
                    "set_fps",
                    new SetFpsPacket(video.fps),
                    VoyagerClient.PORT_SETTINGS,
                    TimeUtils.Epoch + NetUtils.VoyagerClient.TimeOffset);

                buffer.Setup(video.frames);

                lastTimestamp = last;
            }

            if (effect is SyphonStream || effect is SpoutStream)
            {
                var packet = new PacketCollection(
                    new SetStreamPacket(),
                    new SetItshePacket(itshe)
                );

                NetUtils.VoyagerClient.KeepSendingPacket(
                    this,
                    "set_effect",
                    packet,
                    VoyagerClient.PORT_SETTINGS,
                    last
                );
                lastTimestamp = last;
            }

            if (effect is Image image)
            {
                var start = TimeUtils.Epoch + NetUtils.VoyagerClient.TimeOffset;

                var packet = new PacketCollection(
                    new SetVideoPacket(1, start),
                    new SetFpsPacket(1),
                    new SetItshePacket(itshe)
                );

                NetUtils.VoyagerClient.KeepSendingPacket(
                    this,
                    "set_effect",
                    packet,
                    VoyagerClient.PORT_SETTINGS,
                    last);

                NetUtils.VoyagerClient.KeepSendingPacket(
                    this,
                    "set_fps",
                    new SetFpsPacket(1),
                    VoyagerClient.PORT_SETTINGS,
                    TimeUtils.Epoch + NetUtils.VoyagerClient.TimeOffset);

                buffer.Setup(1);
                lastTimestamp = last;
            }

            base.SetEffect(effect);
        }

        public override void SetItshe(Itshe itshe)
        {
            base.SetItshe(itshe);

            var time = TimeUtils.Epoch + NetUtils.VoyagerClient.TimeOffset;
            var packet = new SetItshePacket(itshe);
            var port = VoyagerClient.PORT_SETTINGS;

            NetUtils.VoyagerClient.KeepSendingPacket(this, "set_itshe", packet, port, time);
        }

        public override void PushFrame(Color32[] colors, long frame)
        {
            var data = ColorUtils.ColorsToBytes(colors);
            var packet = new SetFramePacket(frame, data);
            NetUtils.VoyagerClient.SendPacket(this, packet, VoyagerClient.PORT_VIDEO, last);
            base.PushFrame(colors, frame);
        }

        public void PushStreamFrame(Color32[]colors, double time)
        {
            var frame = ColorUtils.ColorsToBytes(colors);
            var packet = new StreamFramePacket(time, frame);
            
            NetUtils.VoyagerClient.SendPacket(this, packet, VoyagerClient.PORT_VIDEO, last);
            
            buffer.Setup(1);
            buffer.SetFrame(0, frame);
            
            prevStream = frame;
        }
    }
}