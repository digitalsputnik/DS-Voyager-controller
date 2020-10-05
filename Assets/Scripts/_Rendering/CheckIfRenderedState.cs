using System.Linq;

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
            var database = ApplicationManager.Lamps;
            return database.GetLamps().All(l =>
            {
                var meta = database.GetMetadata(l.Serial);
                return meta.Effect == null || meta.Effect != null && meta.Rendered;
            });
        }
    }
}