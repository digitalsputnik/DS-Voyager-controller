using System;
using DigitalSputnik.Colors;
using VoyagerController.Effects;

namespace VoyagerController
{
    [Serializable]
    public class LampMetadata
    {
        public DateTime Discovered { get; set; }
        public Effect Effect { get; set; }
        public EffectMapping EffectMapping { get; set; } = new EffectMapping();
        public bool[] ConfirmedFrames { get; set; }
        public double TimeEffectApplied { get; set; }
        public bool Rendered { get; set; } = false;
        public Rgb[][] FrameBuffer { get; set; }
    }
}