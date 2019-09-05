using System.Collections.Generic;
using UnityEngine;
using VoyagerApp.Lamps;
using VoyagerApp.UI.Menus;
using VoyagerApp.Utilities;
using VoyagerApp.Videos;

namespace VoyagerApp.UI
{
    public class VideoMappingController : MonoBehaviour
    {
        [SerializeField] VideoMapper mapper;
        [SerializeField] VideoMappingMenu menu;

        void Start()
        {
            LoadVideoMappingMenu();
        }

        void LoadVideoMappingMenu()
        {
            VideoMappingSettings settings = VideoMappingSettings.Load();

            var video = VideoFromHash(settings.video);
            var lamps = LampsFromSerials(settings.lamps);

            mapper.SetVideo(video);
            menu.SetVideo(video);

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
            return VideoManager.instance.Videos.Find(v => v.hash == hash);
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
    }
}
