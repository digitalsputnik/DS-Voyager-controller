namespace VoyagerController.Rendering
{
    internal class IdleState : VideoRenderState
    {
        internal override VideoRenderState Update()
        {
            return this;
        }
    }
}