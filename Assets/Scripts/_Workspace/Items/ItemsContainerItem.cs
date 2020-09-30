using UnityEngine;

namespace VoyagerController.Workspace
{
    public class ItemsContainerItem : WorkspaceItem
    {
        [SerializeField] private MeshRenderer _outline;
        [SerializeField] private Color _childEnterColor;
        [SerializeField] private Color _normalColor;
        [SerializeField] private float _outlineThickness;

        public void OnChildEnter()
        {
            _outline.material.color = _childEnterColor;
        }

        public void OnChildExit()
        {
            _outline.material.color = _normalColor;
        }
    }
}