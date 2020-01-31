using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VoyagerApp.Effects;
using VoyagerApp.Lamps;
using VoyagerApp.UI.Menus;
using VoyagerApp.Utilities;
using VoyagerApp.Videos;
using VoyagerApp.Workspace;

namespace VoyagerApp.UI
{
    public class VideoMappingController : MonoBehaviour
    {
        [SerializeField] VideoMapper mapper = null;
        [SerializeField] EffectMappingMenu menu = null;

        void Start()
        {
            LoadVideoMappingMenu();
        }

        void LoadVideoMappingMenu()
        {
            EffectMappingSettings settings = EffectMappingSettings.Load();

            var lamps = LampsFromSerials(settings.lamps);
            //var video = VideoFromId(settings.video);

            //SetVideo(video);
            PositionLamps(lamps);
        }

        public void SetVideo(Video video)
        {
            mapper.SetVideo(video);
            menu.SetEffect(video);
        }

        public void PositionLamps(List<Lamp> lamps)
        {
            foreach (var lamp in lamps)
            {
                var position = GetLampVideoPosition(lamp);
                var scale = GetLampVideoScale(lamp);
                var rotation = GetLampVideoRotation(lamp);
                var view = lamp.AddToWorkspace(position, scale, rotation);
                WorkspaceSelection.instance.SelectItem(view);
            }
        }

        Video VideoFromId(string id)
        {
            return EffectManager.GetEffectsOfType<Video>().FirstOrDefault(v => v.id == id);
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
            float x = (lamp.mapping.p1.x + lamp.mapping.p2.x) / 2.0f - 0.5f;
            float y = (lamp.mapping.p1.y + lamp.mapping.p2.y) / 2.0f - 0.5f;
            return mapper.MeshTransform.TransformPoint(x, y, 0);
        }

        float GetLampVideoScale(Lamp lamp)
        {
            Vector2 start = lamp.mapping.p1;
            Vector2 end = lamp.mapping.p2;

            Vector2 wStart = mapper.MeshTransform.TransformPoint(start);
            Vector2 wEnd = mapper.MeshTransform.TransformPoint(end);

            float distance = Vector2.Distance(wStart, wEnd);
            return distance / (lamp.pixels * 0.15f);
        }

        float GetLampVideoRotation(Lamp lamp)
        {
            Vector2 start = lamp.mapping.p1;
            Vector2 end = lamp.mapping.p2;

            Vector2 wStart = mapper.MeshTransform.TransformPoint(start);
            Vector2 wEnd = mapper.MeshTransform.TransformPoint(end);

            return VectorUtils.AngleFromTo(wStart, wEnd);
        }
    }
}
