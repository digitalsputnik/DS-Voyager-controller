using System;
using Newtonsoft.Json;
using UnityEngine;

namespace VoyagerApp.Workspace
{
    [Serializable]
    public class WorkspaceItemSaveData
    {
        [JsonIgnore] public Vector2 position => new Vector2(x, y);

        public string guid;
        public float x;
        public float y;
        public float scale;
        public float rotation;
        public string parentguid;

        public virtual void Load() { }
    }
}
