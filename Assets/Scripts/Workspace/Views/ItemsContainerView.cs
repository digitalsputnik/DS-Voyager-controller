using UnityEngine;

namespace VoyagerApp.Workspace.Views
{
    public class ItemsContainerView : WorkspaceItemView
    {
        [SerializeField] protected MeshRenderer outline;
        [SerializeField] protected Color childEnterColor;
        [SerializeField] protected Color normalColor;
        [SerializeField] protected float outlineThickness;

        public void OnChildEnter()
        {
            outline.material.color = childEnterColor;
        }

        public void OnChildExit()
        {
            outline.material.color = normalColor;
        }
    }
}