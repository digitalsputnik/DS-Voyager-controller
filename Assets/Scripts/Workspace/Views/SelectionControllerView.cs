using Unity.Mathematics;
using UnityEngine;

namespace VoyagerApp.Workspace
{
    public class SelectionControllerView : WorkspaceItemView
    {
        public Transform render = null;

        [SerializeField] SelectionHandle moveHandle = null;
        [SerializeField] SelectionHandle resizeHandle = null;
        [SerializeField] float handleSize = 40.0f;

        Camera cam;

        void Start()
        {
            cam = Camera.main;

            if (Application.isMobilePlatform)
                handleSize *= 2;
        }

        void Update()
        {
            float3 newscale = new float3(1.0f) * cam.orthographicSize * handleSize;
            Rescale(moveHandle.transform, newscale);
            Rescale(resizeHandle.transform, newscale);
        }

        public void SetBounds(Bounds bounds)
        {
            render.localScale = bounds.size;
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

        static void Rescale(Transform obj, Vector3 newScale)
        {
            if (obj.root != obj)
            {
                Transform parent = obj.parent;
                obj.SetParent(null);
                obj.localScale = newScale;
                obj.SetParent(parent, true);
            }
        }
    }
}
