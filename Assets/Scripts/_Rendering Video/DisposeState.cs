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

    internal class IdleState : VideoRenderState
    {
        internal override VideoRenderState Update()
        {
            return this;
        }
    }
}