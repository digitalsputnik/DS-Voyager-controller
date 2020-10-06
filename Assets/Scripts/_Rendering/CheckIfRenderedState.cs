using System.Linq;
using DigitalSputnik;
using DigitalSputnik.Voyager;

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
            return LampManager.Instance.GetLampsOfType<VoyagerLamp>().All(l =>
            {
                var meta = Metadata.Get(l.Serial);
                return meta.Effect == null || meta.Effect != null && meta.Rendered;
            });
        }
    }
}