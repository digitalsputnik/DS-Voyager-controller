using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace VoyagerController.Workspace
{
    public class SelectionMove : MonoBehaviour
    {
        public static bool Enabled = true;

        public static event SelectionHandler OnSelectionMoveStarted;
        public static event SelectionHandler OnSelectionMoveEnded;

        [SerializeField] private SelectionHandle _moveHandle = null;
        [SerializeField] private SelectionHandle _resizeHandle = null;

        private SelectionControllerItem _targetView;

        private float _zPos;
        private Camera _camera;

        private Vector2 _startPosition;
        private float _angleOffset;
        private float _startDistance;
        private float _startScale;

        private List<ItemsContainerItem> _prevUnder = new List<ItemsContainerItem>();

        public static void RaiseMovedEvent() => OnSelectionMoveEnded?.Invoke();

        private void Start()
        {
            _targetView = GetComponentInParent<SelectionControllerItem>();
            _zPos = TargetTransform.position.z;
            _camera = Camera.main;
            _moveHandle.SetListener(OnMoveHandle);
            _resizeHandle.SetListener(OnResizeHandle);
        }

        private void OnMoveHandle(SelectionHandleState state)
        {
            if (state.phase == SelectionHandlerPhase.Begin)
            {
                _startPosition = TargetPos;
                OnSelectionMoveStarted?.Invoke();
            }

            var delta = state.startPosition - state.position;
            TargetPos = _startPosition - delta;

            CheckForItems(state.position);

            if (state.phase == SelectionHandlerPhase.End)
            {
                ManageOldUnder(_prevUnder);
                OnSelectionMoveEnded?.Invoke();
            }
        }

        private void OnResizeHandle(SelectionHandleState state)
        {
            if (state.phase == SelectionHandlerPhase.Begin)
            {
                _startPosition = TargetPos;

                var angleStart = _startPosition.AngleTo(state.position);
                _angleOffset = TargetRotation - angleStart;

                _startDistance = math.distance(_startPosition, state.position);
                _startScale = TargetScale;

                OnSelectionMoveStarted?.Invoke();
            }

            var rotation = _startPosition.AngleTo(state.position);
            TargetRotation = rotation + _angleOffset;

            var distance = math.distance(_startPosition, state.position);
            var factor = distance / _startDistance;
            TargetScale = _startScale * factor;

            if (state.phase == SelectionHandlerPhase.End)
                OnSelectionMoveEnded?.Invoke();
        }

        private Transform TargetTransform => _targetView.transform;

        private Vector2 TargetPos
        {
            get
            {
                var position = TargetTransform.position;
                return new Vector2(position.x, position.y);
            }
            set => TargetTransform.position = new Vector3(value.x, value.y, _zPos);
        }

        private float TargetRotation
        {
            get => TargetTransform.eulerAngles.z;
            set => TargetTransform.eulerAngles = new Vector3(0.0f, 0.0f, value);
        }

        private float TargetScale
        {
            get => TargetTransform.localScale.x;
            set => TargetTransform.localScale = Vector3.one * value;
        }

        void CheckForItems(Vector2 pressPosition)
        {
            var under = GetItemsUnderMouse(pressPosition);
            var newU = under.Where(u => !_prevUnder.Contains(u)).ToList();
            var oldU = _prevUnder.Where(u => !under.Contains(u)).ToList();

            ManageNewUnder(newU);
            ManageOldUnder(oldU);

            _prevUnder = under;
        }

        private List<ItemsContainerItem> GetItemsUnderMouse(Vector2 pressPosition)
        {
            var under = new List<ItemsContainerItem>();
            var hits = Physics2D.RaycastAll(pressPosition, Vector2.zero);
            foreach (var hit in hits)
            {
                var trans = hit.collider?.transform;
                if (trans != null)
                {
                    var item = trans.parent?.GetComponent<ItemsContainerItem>();
                    if (item != null && item != _targetView)
                        under.Add(item);
                }
            }
            return under;
        }

        private static void ManageNewUnder(List<ItemsContainerItem> under) => under.ForEach(_ => _.OnChildEnter());

        private static void ManageOldUnder(List<ItemsContainerItem> under) => under.ForEach(_ => _.OnChildExit());
    }

    public delegate void SelectionHandler();
}