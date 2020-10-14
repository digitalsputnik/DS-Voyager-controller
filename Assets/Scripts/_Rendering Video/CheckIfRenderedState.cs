using System.Linq;
using DigitalSputnik;
using DigitalSputnik.Voyager;
using VoyagerController.Effects;

namespace VoyagerController.Rendering
{
    internal class CheckIfRenderedState : VideoRenderState
    {
        internal override VideoRenderState Update()
        {
            if (!AllLampsRendered())
                return new PrepareState();
            return this;
        }

        private static bool AllLampsRendered()
        {
            return LampManager.Instance.GetLampsOfType<VoyagerLamp>().ToArray().All(l =>
            {
                var meta = Metadata.Get(l.Serial);
                return !(meta.Effect is VideoEffect) || meta.Effect is VideoEffect && meta.Rendered;
            });
        }
    }
}