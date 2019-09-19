using UnityEngine;

namespace VoyagerApp.Workspace
{
    public class SelectionControllerView : WorkspaceItemView
    {
        [SerializeField] Transform render = null;

        public void SetBounds(Bounds bounds)
        {
            render.localScale = bounds.size;
            transform.position = bounds.center;
        }
    }
}
