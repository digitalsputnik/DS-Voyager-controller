using System.Linq;
using DigitalSputnik;
using DigitalSputnik.Voyager;
using VoyagerController.Effects;

namespace VoyagerController.Rendering
{
    internal class PrepareState : VideoRenderState
    {
        internal override VideoRenderState Update()
        {
            var unRendered = LampManager.Instance.GetLampsOfType<VoyagerLamp>()
                .Where(l =>
                {
                    var meta = Metadata.Get(l.Serial);
                    return meta.Effect is VideoEffect && !meta.Rendered;
                })
                .ToArray();
            
            if (!unRendered.Any())
                return new IdleState();

            return new RenderState(RenderQueue.Create(unRendered));
        }
    }
}