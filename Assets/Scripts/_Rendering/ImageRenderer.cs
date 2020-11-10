using System.Collections.Generic;
using System.Linq;
using DigitalSputnik.Voyager;
using UnityEngine;
using VoyagerController.Effects;
using VoyagerController.Workspace;

namespace VoyagerController.Rendering
{
    public class ImageRenderer : MonoBehaviour
    {
        private void Update()
        {
            foreach (var voyager in LampsWithImageEffectNotRendered)
                RenderImage(voyager);
        }
        
        private static void RenderImage(VoyagerLamp voyager)
        {
            if (Metadata.Get(voyager.Serial).Effect is ImageEffect effect)
            {
                var image = effect.ImageTexture;
                var coords = TextureExtensions.MapLampToVideoCoords(voyager, image);
                var rgb =  TextureExtensions.CoordsToColors(coords.ToArray(), image).ToRgbArray();
                LampEffectsWorker.ApplyVideoFrameToVoyager(voyager, 0, rgb);
            }
        }
        
        private static IEnumerable<VoyagerLamp> LampsWithImageEffectNotRendered => WorkspaceManager
            .GetItems<VoyagerItem>()
            .Where(v =>
            {
                var meta = Metadata.Get(v.LampHandle.Serial);
                return meta.Effect is ImageEffect && !meta.ConfirmedFrames[0];
            })
            .Select(i => i.LampHandle);
    }
}