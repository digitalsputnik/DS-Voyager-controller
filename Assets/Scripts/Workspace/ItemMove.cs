using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using VoyagerApp.Workspace.Views;

namespace VoyagerApp.Workspace
{
    public class ItemMove : MonoBehaviour
    {
        public static event ItemMoveHandler onItemMoveStarted;
        public static event ItemMoveHandler onItemMoveEnded;
        public static bool Enabled = true;

        Transform target;
        WorkspaceItemView targetItem;

        Vector2 pressStartPosition;
        Vector2 objectStartPosition;

        float startAngle;
        float offsetAngle;

        float startDistance;
        Vector2 startScale;

        Camera cam;
        bool moving;
        bool approved;

        List<ItemsContainerView> prevUnder = new List<ItemsContainerView>();

        void Start()
        {
            targetItem = GetComponentInParent<WorkspaceItemView>();
            target = targetItem.transform;
            cam = Camera.main;
        }

        void OnMouseDown()
        {
            approved = !PointerOverUI && Enabled;

            if (approved)
            {
                Vector2 pressPosition = cam.ScreenToWorldPoint(Input.mousePosition);

                pressStartPosition = pressPosition - (Vector2)transform.position;
                objectStartPosition = target.position;

                startAngle = AngleFromTo(objectStartPosition, pressPosition);
                offsetAngle = startAngle - target.eulerAngles.z;

                startDistance = Vector2.Distance(objectStartPosition, pressPosition);
                startScale = target.localScale;

                float scaleDistance = transform.lossyScale.x * 0.85f / 2.0f;
                moving = startDistance < scaleDistance;

                prevUnder.Clear();

                if (approved) onItemMoveStarted?.Invoke(this);
            }
        }

        void OnMouseDrag()
        {
            if (!approved) return;

            Vector2 pressPosition = cam.ScreenToWorldPoint(Input.mousePosition);

            if (moving)
            {
                HandleMoving(pressPosition);
                CheckForItems(pressPosition);
            }
            else
            {
                HandleRotation(pressPosition);
                HandleScale(pressPosition);
            }
        }

        void OnMouseUp()
        {
            if (!approved) return;

            onItemMoveEnded?.Invoke(this);

            if (moving)
            {
                WorkspaceItemView self = target.GetComponent<WorkspaceItemView>();
                self.SetParent(prevUnder.Count > 0 ? prevUnder[0] : null);
                ManageOldUnder(prevUnder);
            }
        }

        void HandleMoving(Vector2 pressPosition)
        {
            Vector2 delta = pressStartPosition - pressPosition;
            Vector3 position = -delta;
            position.z = transform.position.z;
            target.position = position;
        }

        void HandleRotation(Vector2 pressPosition)
        {
            float rotation = AngleFromTo(objectStartPosition, pressPosition);
            target.eulerAngles = new Vector3(0, 0, rotation - offsetAngle);
        }

        void HandleScale(Vector2 pressPosition)
        {
            float distance = Vector2.Distance(objectStartPosition, pressPosition);
            float factor = distance / startDistance;
            Vector3 scale = startScale * factor;
            scale.z = 1.0f;
            target.localScale = scale;
        }

        float AngleFromTo(Vector2 from, Vector2 to)
        {
            Vector2 direction = to - from;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            if (angle < 0f) angle += 360f;
            return angle;
        }

        void CheckForItems(Vector2 pressPosition)
        {
            var under = GetItemsUnderMouse(pressPosition);
            var newU = under.Where(u => !prevUnder.Contains(u)).ToList();
            var oldU = prevUnder.Where(u => !under.Contains(u)).ToList();

            ManageNewUnder(newU);
            ManageOldUnder(oldU);

            prevUnder = under;
        }

        List<ItemsContainerView> GetItemsUnderMouse(Vector2 pressPosition)
        {
            var under = new List<ItemsContainerView>();
            var hits = Physics2D.RaycastAll(pressPosition, Vector2.zero);
            foreach (var hit in hits)
            {
                Transform trans = hit.collider?.transform;
                if (trans != null)
                {
                    var item = trans.parent.GetComponent<ItemsContainerView>();
                    if (item != null && item != targetItem)
                        under.Add(item);
                }
            }
            return under;
        }

        void ManageNewUnder(List<ItemsContainerView> under)
        {
            under.ForEach(_ => _.OnChildEnter());
        }

        void ManageOldUnder(List<ItemsContainerView> under)
        {
            under.ForEach(_ => _.OnChildExit());
        }

        bool PointerOverUI
        {
            get
            {
                if (Application.isMobilePlatform)
                {
                    for (int i = 0; i < Input.touchCount; i++)
                    {
                        int id = Input.GetTouch(i).fingerId;
                        if (EventSystem.current.IsPointerOverGameObject(id))
                            return true;
                    }
				}
                return EventSystem.current.IsPointerOverGameObject();
            }
        }
    }

    public delegate void ItemMoveHandler(ItemMove item);
}