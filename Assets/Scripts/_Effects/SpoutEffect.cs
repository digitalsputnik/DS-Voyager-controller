namespace VoyagerController.Effects
{
    public class SpoutEffect : Effect
    {
        public override string Name => "Spout";
        public string Source { get; set; }
        public double Delay { get; set; } = 0.04;
    }
}