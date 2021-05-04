using System;

namespace VoyagerController
{
    [Serializable]
    public class WorkspaceMapping
    {
        public float[] Position { get; set; }
        public float Rotation { get; set; }
        public float Scale { get; set; }

        public WorkspaceMapping()
        {
            Position = new[] { 0.0f, 0.0f };
            Rotation = 0.0f;
            Scale = 1.0f;
        }
    }
}