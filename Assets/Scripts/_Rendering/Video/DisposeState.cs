namespace VoyagerController.Rendering
{
    internal class DisposeState : VideoRenderState
    {
        internal override VideoRenderState Update()
        {
            VideoEffectRenderer.Clear();
            return new IdleState();
        }
    }
}