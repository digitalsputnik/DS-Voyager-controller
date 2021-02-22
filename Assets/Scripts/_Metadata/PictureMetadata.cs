using System;
using UnityEngine;

namespace VoyagerController
{
    [Serializable]
    public class PictureMetadata
    {
        public Texture2D Texture { get; set; }
        public WorkspaceMapping WorkspaceMapping { get; set; } = new WorkspaceMapping();
    }
}