using Unity.Mathematics;
using UnityEngine;

namespace VoyagerApp.Workspace
{
    public interface ISelectableItem
    {
        bool Selected { get; }
        void Select();
        void Deselect();
        float3[] SelectPositions { get; }
        Bounds Bounds { get; }
        WorkspaceItemView View { get; }
    }
}
