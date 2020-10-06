namespace VoyagerController
{
    public class EffectMapping
    {
        public float[] Positions { get; set; } = { 0.0f, 0.5f, 1.0f, 0.5f };

        public float X1
        {
            get => Positions[0];
            set => Positions[0] = value;
        }
        
        public float Y1
        {
            get => Positions[1];
            set => Positions[1] = value;
        }
        
        public float X2
        {
            get => Positions[2];
            set => Positions[2] = value;
        }
        
        public float Y2
        {
            get => Positions[3];
            set => Positions[3] = value;
        }
    }
}