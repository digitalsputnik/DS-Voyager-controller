using System.Linq;
using VoyagerController.Effects;

namespace VoyagerController.Rendering
{
    internal class PrepareState : VideoRenderState
    {
        internal override VideoRenderState Update()
        {
            var database = ApplicationManager.Lamps;
            var unRendered = database.GetLamps()
                .Where(l =>
                {
                    var meta = database.GetMetadata((string) l.Serial);
                    return meta.Effect is VideoEffect && !meta.Rendered;
                });

            return new RenderState(RenderQueue.Create(unRendered));
        }
    }
}