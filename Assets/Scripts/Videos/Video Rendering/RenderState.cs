namespace VoyagerApp.Videos
{
    public abstract class RenderState
    {
        public abstract void HandleEvent(VideoRenderEvent type);
        public abstract RenderState Update();
        public virtual void OnCancel() { }
    }
}
