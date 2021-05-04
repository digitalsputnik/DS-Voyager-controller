namespace VoyagerController.Effects
{
    public class SyphonEffect : Effect
    {
        public override string Name => "Syphon";
        public SyphonCredentials Server { get; set; }
        public double Delay { get; set; } = 0.15;
    }
}