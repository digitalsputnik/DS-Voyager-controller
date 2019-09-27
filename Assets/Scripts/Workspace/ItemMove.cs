using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VoyagerApp.UI;
using VoyagerApp.Utilities;
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

        float zPos;

        Camera cam;
        bool moving;
        bool active;

        List<ItemsContainerView> prevUnder = new List<ItemsContainerView>();
        CameraMoveState prevState;

        void Start()
        {
            targetItem = GetComponentInParent<WorkspaceItemView>();
            target = targetItem.transform;
            zPos = target.transform.position.z;
            cam = Camera.main;
        }

        void Update()
        {
            if (!Enabled)
            {
                if (prevState == CameraMoveState.WorkspaceOverItem)
                    OnMoveEnded();
                return;
            }

            if (prevState != CameraMoveState.WorkspaceOverItem &&
                CameraMove.state == CameraMoveState.WorkspaceOverItem)
                OnMoveStarted();

            if (prevState == CameraMoveState.WorkspaceOverItem &&
                CameraMove.state != CameraMoveState.WorkspaceOverItem)
                OnMoveEnded();

            if (active)
                OnMove();

            prevState = CameraMove.state;
        }

        void OnMoveStarted()
        {
            var bounds = GetComponent<BoxCollider2D>().bounds;
            bounds.Expand(Vector3.forward * 100.0f);

            var hits = Physics2D.RaycastAll(cam.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            bool contains = false;
            foreach (var hit in hits)
                if (hit.transform == transform) contains = true;

            if (!contains || CameraMove.Used)
                return;

            if (WorkspaceUtils.SelectedLamps.Count > 1 &&
                WorkspaceManager.instance.GetItemsOfType<SelectionControllerView>().Length != 0)
                if (targetItem is LampItemView) return;

            Vector2 pressPosition = CameraMove.pointerPosition;

            pressStartPosition = pressPosition - (Vector2)transform.position;
            objectStartPosition = target.position;

            startAngle = AngleFromTo(objectStartPosition, pressPosition);
            offsetAngle = startAngle - target.eulerAngles.z;

            startDistance = Vector2.Distance(objectStartPosition, pressPosition);
            startScale = target.localScale;

            float longest =
                transform.lossyScale.x > transform.lossyScale.y ?
                transform.lossyScale.x : transform.lossyScale.y;

            float scaleDistance = longest * 0.85f / 2.0f;
            moving = startDistance < scaleDistance;

            prevUnder.Clear();
            onItemMoveStarted?.Invoke(this);

            CameraMove.Used = true;
            active = true;
        }


        void OnMove()
        {
            Vector2 pressPosition = CameraMove.pointerPosition;

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

        void OnMoveEnded()
        {
            if (!active) return;

            onItemMoveEnded?.Invoke(this);
            if (moving)
            {
                WorkspaceItemView self = target.GetComponent<WorkspaceItemView>();
                self.SetParent(prevUnder.Count > 0 ? prevUnder[0] : null);
                ManageOldUnder(prevUnder);
            }

            CameraMove.Used = false;
            active = false;
        }

        void HandleMoving(Vector2 pressPosition)
        {
            Vector2 delta = pressStartPosition - pressPosition;
            Vector3 position = -delta;
            position.z = zPos;
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
    }

    public delegate void ItemMoveHandler(ItemMove item);
}