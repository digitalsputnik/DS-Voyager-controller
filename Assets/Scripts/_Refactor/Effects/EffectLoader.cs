namespace VoyagerApp.RefactoredEffects
{
    public static class EffectLoader
    {
        public static void LoadVideoEffect(string path, EffectHandler loaded)
        {
            App.VideoTools.LoadVideo(path, video => loaded?.Invoke(new VideoEffect(video)));
        }
    }
}