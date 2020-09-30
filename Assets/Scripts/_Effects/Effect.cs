namespace VoyagerController.Effects
{
    public delegate void EffectHandler(Effect effect);
    
    public abstract class Effect
    {
        public string Id { get; set; }
        public abstract string Name { get; }
        public EffectSettings Settings = new EffectSettings();
        public EffectMeta Meta = new EffectMeta();
    }
}