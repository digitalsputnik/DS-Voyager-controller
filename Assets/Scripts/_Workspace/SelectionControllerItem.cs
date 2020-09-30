using UnityEngine;

namespace VoyagerController.Workspace
{
    public class SelectionControllerItem : WorkspaceItem
    {
        [SerializeField] private Transform _render = null;

        [SerializeField] private SelectionHandle moveHandle = null;
        [SerializeField] private SelectionHandle resizeHandle = null;
        [SerializeField] private float _handleSize = 40.0f;

        public override bool Selectable => false;

        private Camera _cam;

        private void Start()
        {
            _cam = Camera.main;

            if (Application.isMobilePlatform)
                _handleSize *= 2;
        }

        private void Update()
        {
            var newscale = Vector3.one * _cam.orthographicSize * _handleSize;
            Rescale(moveHandle.transform, newscale);
            Rescale(resizeHandle.transform, newscale);
        }

        public void SetBounds(Bounds bounds)
        {
            _render.localScale = bounds.size;
            transform.position = bounds.center;

            if (bounds.size.x > bounds.size.y)
            {
                resizeHandle.transform.position = new Vector3(
                    transform.position.x + bounds.size.x / 2.0f,
                    transform.position.y,
                    transform.position.z - 0.1f
                );
            }
            else
            {
                resizeHandle.transform.position = new Vector3(
                    transform.position.x,
                    transform.position.y - bounds.size.y / 2.0f,
                    transform.position.z - 0.1f 
                );
            }
        }

        private static void Rescale(Transform obj, Vector3 newScale)
        {
            if (obj.root == obj) return;
            
            var parent = obj.parent;
            obj.SetParent(null);
            obj.localScale = newScale;
            obj.SetParent(parent, true);
        }
    }
}
