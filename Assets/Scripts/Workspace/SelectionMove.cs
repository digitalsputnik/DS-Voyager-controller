using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using VoyagerApp.Utilities;
using VoyagerApp.Workspace.Views;

namespace VoyagerApp.Workspace
{
    public class SelectionMove : MonoBehaviour
    {
        public static new bool enabled = true;

        public static event SelectionHandler onSelectionMoveStarted;
        public static event SelectionHandler onSelectionMoveEnded;

        [SerializeField] SelectionHandle moveHandle = null;
        [SerializeField] SelectionHandle resizeHandle = null;

        SelectionControllerView targetView;

        float zPos;
        Camera cam;

        float2 startPosition;
        float angleOffset;
        float startDistance;
        float startScale;

        List<ItemsContainerView> prevUnder = new List<ItemsContainerView>();

        public static void RaiseMovedEvent()
        {
            onSelectionMoveEnded?.Invoke();
        }

        void Start()
        {
            targetView = GetComponentInParent<SelectionControllerView>();
            zPos = targetTransform.position.z;
            cam = Camera.main;

            moveHandle.SetListener(OnMoveHandle);
            resizeHandle.SetListener(OnResizeHandle);
        }

        void OnMoveHandle(SelectionHandleState state)
        {
            if (state.phase == SelectionHandlerPhase.Begin)
            {
                startPosition = targetPos;
                onSelectionMoveStarted?.Invoke();
            }

            float2 delta = state.startPosition - state.position;
            targetPos = startPosition - delta;

            CheckForItems(state.position);

            if (state.phase == SelectionHandlerPhase.End)
            {
                ManageOldUnder(prevUnder);
                onSelectionMoveEnded?.Invoke();
            }
        }

        void OnResizeHandle(SelectionHandleState state)
        {
            if (state.phase == SelectionHandlerPhase.Begin)
            {
                startPosition = targetPos;

                float angleStart = VectorUtils.AngleFromTo(startPosition, state.position);
                angleOffset = targetRotation - angleStart;

                startDistance = math.distance(startPosition, state.position);
                startScale = targetScale;

                onSelectionMoveStarted?.Invoke();
            }

            float rotation = VectorUtils.AngleFromTo(startPosition, state.position);
            targetRotation = rotation + angleOffset;

            float distance = math.distance(startPosition, state.position);
            float factor = distance / startDistance;
            targetScale = startScale * factor;

            if (state.phase == SelectionHandlerPhase.End)
                onSelectionMoveEnded?.Invoke();
        }

        Transform targetTransform => targetView.transform;

        float2 targetPos
        {
            get => new float2((Vector2)targetTransform.position);
            set => targetTransform.position = new float3(value, zPos);
        }

        float targetRotation
        {
            get => targetTransform.eulerAngles.z;
            set => targetTransform.eulerAngles = new float3(float2.zero, value);
        }

        float targetScale
        {
            get => targetTransform.localScale.x;
            set => targetTransform.localScale = new float3(1.0f) * value;
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
                    var item = trans.parent?.GetComponent<ItemsContainerView>();
                    if (item != null && item != targetView)
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

    public delegate void SelectionHandler();
}