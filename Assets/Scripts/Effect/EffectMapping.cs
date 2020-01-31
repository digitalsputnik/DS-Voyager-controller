using Unity.Mathematics;

namespace VoyagerApp.Effects
{
    public struct EffectMapping
    {
        public float2 p1;
        public float2 p2;

        public EffectMapping(float2 p1, float2 p2)
        {
            this.p1 = p1;
            this.p2 = p2;
        }

        public static EffectMapping Default
        {
            get
            {
                return new EffectMapping
                {
                    p1 = new float2(0.0f, 0.5f),
                    p2 = new float2(1.0f, 0.5f)
                };
            }
        }
    }
}