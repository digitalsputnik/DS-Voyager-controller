using System;
using DigitalSputnik.Colors;
using VoyagerController.Effects;

namespace VoyagerController
{
    [Serializable]
    public class LampMetadata
    {
        public double Discovered { get; set; } = 0.0;
        public Effect Effect { get; set; }
        public EffectMapping EffectMapping { get; set; } = new EffectMapping();
        public bool InWorkspace { get; set; } = false;
        public WorkspaceMapping WorkspaceMapping { get; set; } = new WorkspaceMapping();
        public Itshe Itshe { get; set; } = new Itshe();
        public bool[] ConfirmedFrames { get; set; } = new bool[0];
        public long TotalMissingFrames { get; set; } = 0;
        public Rgb[] PreviousStreamFrame { get; set; }
        public double TimeEffectApplied { get; set; }
        public double VideoStartTime { get; set; }
        public bool Rendered { get; set; } = false;
        public Rgb[][] FrameBuffer { get; set; }

        public LampMetadata()
        {
            Itshe.E = 1.0f;
        }
    }
}