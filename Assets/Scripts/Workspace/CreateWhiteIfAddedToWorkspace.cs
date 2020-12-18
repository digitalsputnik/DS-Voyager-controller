using System.Collections;
using UnityEngine;
using VoyagerApp.Effects;
using VoyagerApp.Lamps;
using VoyagerApp.Lamps.Voyager;
using VoyagerApp.Networking.Voyager;
using VoyagerApp.Utilities;
using VoyagerApp.Workspace;
using VoyagerApp.Workspace.Views;

namespace VoyagerApp.UI
{
    public class CreateWhiteIfAddedToWorkspace : MonoBehaviour
    {
        void Start()
        {
            WorkspaceManager.instance.onItemAdded += OnLampAddedToWorkspace;
        }

        void OnDestroy()
        {
            WorkspaceManager.instance.onItemAdded -= OnLampAddedToWorkspace;
        }

        void OnLampAddedToWorkspace(WorkspaceItemView item)
        {
            if (item is LampItemView lampView)
                StartCoroutine(AddLamp(lampView.lamp));
        }

        IEnumerator AddLamp(Lamp lamp)
        {
            yield return new WaitForSeconds(0.3f);
            var video = EffectManager.GetEffectWithName<Video>("white");

            if (lamp is VoyagerLamp voyager)
            {
                if (lamp.effect == null && !voyager.dmxEnabled)
                {
                    yield return new WaitUntil(() => video.available.value);

                    lamp.SetMapping(EffectMapping.Default);
                    lamp.SetEffect(video);
                    lamp.SetItshe(ApplicationSettings.AddedLampsDefaultColor);

                    var handle = TimeUtils.Epoch + NetUtils.VoyagerClient.TimeOffset + 0.3;

                    NetUtils.VoyagerClient.SendPacket(
                        lamp,
                        new SetPlayModePacket(PlaybackMode.Play,  video.startTime, handle),
                        VoyagerClient.PORT_SETTINGS);
                }   
            }
        }
    }
}
