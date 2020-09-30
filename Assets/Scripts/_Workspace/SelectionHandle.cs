using System;
using UnityEngine;

namespace VoyagerController.Workspace
{
    public class SelectionHandle : MonoBehaviour
    {
        private Action<SelectionHandleState> _listener;
        private CameraMoveState _prevState = CameraMoveState.None;
        private Vector2 _startPosition;
        private bool _active;

        public void SetListener(Action<SelectionHandleState> action)
        {
            _listener = action;
        }

        private void Update()
        {
            if (StateChangedTo(CameraMoveState.WorkspaceOverItem) && Valid && !CameraMove.Used)
                StartMove();

            if (_active) Move();

            if (StateChangedFrom(CameraMoveState.WorkspaceOverItem) && _active)
                EndMove();

            _prevState = CameraMove.State;
        }

        private void StartMove()
        {
            _startPosition = CameraMove.PointerPosition;

            _listener?.Invoke(new SelectionHandleState
            {
                phase = SelectionHandlerPhase.Begin,
                startPosition = _startPosition,
                position = _startPosition
            });

            _active = true;
        }

        private void Move()
        {
            var point = CameraMove.PointerPosition;

            _listener?.Invoke(new SelectionHandleState
            {
                phase = SelectionHandlerPhase.Moved,
                startPosition = _startPosition,
                position = point
            });
        }

        private void EndMove()
        {
            Vector2 point = CameraMove.PointerPosition;

            _listener?.Invoke(new SelectionHandleState
            {
                phase = SelectionHandlerPhase.End,
                startPosition = _startPosition,
                position = point
            });

            _active = false;
        }

        private bool Valid
        {
            get
            {
                var hits = Physics2D.RaycastAll(CameraMove.PointerPosition, Vector2.zero);
                var contains = false;
                foreach (var hit in hits)
                    if (hit.transform == transform) contains = true;
                return contains && SelectionMove.Enabled;
            }
        }

        private bool StateChangedTo(CameraMoveState state)
        {
            return _prevState != state && CameraMove.State == state;
        }

        private bool StateChangedFrom(CameraMoveState state)
        {
            return _prevState == state && CameraMove.State != state;
        }
    }

    [Serializable]
    public struct SelectionHandleState
    {
        public SelectionHandlerPhase phase;
        public Vector2 position;
        public Vector2 startPosition;
    }

    public enum SelectionHandlerPhase
    {
        Begin, Moved, End
    }
}