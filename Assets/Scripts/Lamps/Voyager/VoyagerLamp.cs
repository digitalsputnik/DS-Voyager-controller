using System;
using System.Net;
using UnityEngine;
using VoyagerApp.Networking;
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

        public void SetItshWithVideo(Itsh itsh)
        {
            base.SetItsh(itsh);
            client = NetworkManager.instance.GetLampClient<VoyagerClient>();
            client.SendVideoMetadata(this);
        }

        public void SetItshWithoutVideo(Itsh itsh)
        {
            base.SetItsh(itsh);
            client = NetworkManager.instance.GetLampClient<VoyagerClient>();
            client.SendItshAsMetadata(this);
            buffer.RecreateBuffer(0);
        }

        public override void SetVideo(Video video)
        {
            base.SetVideo(video);
            client = NetworkManager.instance.GetLampClient<VoyagerClient>();
            client.SendVideoMetadata(this);

            buffer.RecreateBuffer(video.frames);
            Debug.Log($"HERE {serial}");
        }

        public override void PushFrame(Color32[] colors, long frame)
        {
            base.PushFrame(colors, frame);

            if (client == null)
                client = NetworkManager.instance.GetLampClient<VoyagerClient>();
            client.SendVideoFrameData(this, frame, colors);
        }
    }
}