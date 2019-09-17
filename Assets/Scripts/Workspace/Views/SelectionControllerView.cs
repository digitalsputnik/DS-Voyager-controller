using UnityEngine;

namespace VoyagerApp.Workspace
{
    public class SelectionControllerView : WorkspaceItemView
    {
        [SerializeField] Transform render;

        public void SetBounds(Bounds bounds)
        {
            render.localScale = bounds.size;
            transform.position = bounds.center;
        }
    }
}
