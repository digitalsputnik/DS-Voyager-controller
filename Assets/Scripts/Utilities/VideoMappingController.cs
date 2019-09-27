using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using VoyagerApp.Lamps;
using VoyagerApp.Networking.Packages;
using VoyagerApp.Networking.Packages.Voyager;
using VoyagerApp.UI.Menus;
using VoyagerApp.Utilities;
using VoyagerApp.Videos;
using VoyagerApp.Workspace;

namespace VoyagerApp.UI
{
    public class VideoMappingController : MonoBehaviour
    {
        [SerializeField] VideoMapper mapper = null;
        [SerializeField] VideoMappingMenu menu = null;

        void Start()
        {
            LoadVideoMappingMenu();
            WorkspaceSelection.instance.Enabled = true;
            WorkspaceSelection.instance.ShowSelection = true;
        }

        void LoadVideoMappingMenu()
        {
            VideoMappingSettings settings = VideoMappingSettings.Load();

            var lamps = LampsFromSerials(settings.lamps);
            var video = VideoFromHash(settings.video);

            SetVideo(video);
            PositionLamps(lamps);

            if (video == null) StartCoroutine(SetWhiteColor(lamps));
        }

        public void SetVideo(Video video)
        {
            mapper.SetVideo(video);
            menu.SetVideo(video);
        }

        public void PositionLamps(List<Lamp> lamps)
        {
            foreach (var lamp in lamps)
            {
                if (lamp.mapping == null)
                    lamp.SetMapping(new VideoPosition());

                var position = GetLampVideoPosition(lamp);
                var scale = GetLampVideoScale(lamp);
                var rotation = GetLampVideoRotation(lamp);
                lamp.AddToWorkspace(position, scale, rotation);
            }
        }

        Video VideoFromHash(string hash)
        {
            return VideoManager.instance.Videos.FirstOrDefault(v => v.hash == hash);
        }

        List<Lamp> LampsFromSerials(string[] serials)
        {
            List<Lamp> lamps = new List<Lamp>();

            foreach (var serial in serials)
            {
                Lamp lamp = LampManager.instance.GetLampWithSerial(serial);
                if (lamp != null) lamps.Add(lamp);
            }

            return lamps;
        }

        Vector2 GetLampVideoPosition(Lamp lamp)
        {
            float x = (lamp.mapping.x1 + lamp.mapping.x2) / 2.0f - 0.5f;
            float y = (lamp.mapping.y1 + lamp.mapping.y2) / 2.0f - 0.5f;
            return mapper.MeshTransform.TransformPoint(x, y, 0);
        }

        float GetLampVideoScale(Lamp lamp)
        {
            Vector2 start  = new Vector2(lamp.mapping.x1, lamp.mapping.y1);
            Vector2 end    = new Vector2(lamp.mapping.x2, lamp.mapping.y2);

            Vector2 wStart = mapper.MeshTransform.TransformPoint(start);
            Vector2 wEnd   = mapper.MeshTransform.TransformPoint(end);

            float distance = Vector2.Distance(wStart, wEnd);
            return distance / (lamp.pixels * 0.15f);
        }

        float GetLampVideoRotation(Lamp lamp)
        {
            Vector2 start  = new Vector2(lamp.mapping.x1, lamp.mapping.y1);
            Vector2 end    = new Vector2(lamp.mapping.x2, lamp.mapping.y2);

            Vector2 wStart = mapper.MeshTransform.TransformPoint(start);
            Vector2 wEnd   = mapper.MeshTransform.TransformPoint(end);

            return VectorUtils.AngleFromTo(wStart, wEnd);
        }

        IEnumerator SetWhiteColor(List<Lamp> lamps)
        {
            Itshe itshe = new Itshe(Color.white, 1.0f);
            Video video = new Video();
            video.frames = 1;

            double time = TimeUtils.Epoch + NetUtils.VoyagerClient.TimeOffset;

            foreach (var lamp in lamps)
            {
                var start = video.lastStartTime + NetUtils.VoyagerClient.TimeOffset;
                var packet = new PacketCollection(
                    new SetVideoPacket(video.frames, start),
                    new SetItshePacket(itshe)
                );
                NetUtils.VoyagerClient.SendPacket(lamp, packet, time);
            }

            yield return new WaitForSeconds(0.3f);

            foreach (var lamp in lamps)
            {
                lamp.itshe = itshe;
                byte[] colors = ColorUtils.LampFrameBufferFromColor(lamp, Color.white);
                var packet = new SetFramePacket(0, colors);
                NetUtils.VoyagerClient.SendPacketToVideoPort(lamp, packet, time);
            }
        }
    }
}
