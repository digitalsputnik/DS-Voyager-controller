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
        public Itshe Itshe { get; set; } = new Itshe();
        public bool[] ConfirmedFrames { get; set; }
        public Rgb[] PreviousStreamFrame { get; set; }
        public double TimeEffectApplied { get; set; }
        public double VideoStartTime { get; set; }
        public bool Rendered { get; set; } = false;
        public Rgb[][] FrameBuffer { get; set; }
    }
}