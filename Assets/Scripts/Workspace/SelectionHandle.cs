using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using VoyagerApp.UI;

namespace VoyagerApp.Workspace
{
    public class SelectionHandle : MonoBehaviour
    {
        Action<SelectionHandleState> listener;
        CameraMoveState prevState = CameraMoveState.None;

        float2 startPosition;
        bool active;

        public void SetListener(Action<SelectionHandleState> action)
        {
            listener = action;
        }

        void Update()
        {
            if (StateChangedTo(CameraMoveState.WorkspaceOverItem) && Valid && !CameraMove.Used)
                StartMove();

            if (active) Move();

            if (StateChangedFrom(CameraMoveState.WorkspaceOverItem) && active)
                EndMove();

            prevState = CameraMove.state;
        }

        void StartMove()
        {
            startPosition = CameraMove.pointerPosition;

            listener?.Invoke(new SelectionHandleState
            {
                phase = SelectionHandlerPhase.Begin,
                startPosition = startPosition,
                position = startPosition
            });

            active = true;
        }

        void Move()
        {
            float2 point = CameraMove.pointerPosition;

            listener?.Invoke(new SelectionHandleState
            {
                phase = SelectionHandlerPhase.Moved,
                startPosition = startPosition,
                position = point
            });
        }

        void EndMove()
        {
            float2 point = CameraMove.pointerPosition;

            listener?.Invoke(new SelectionHandleState
            {
                phase = SelectionHandlerPhase.End,
                startPosition = startPosition,
                position = point
            });

            active = false;
        }

        bool Valid
        {
            get
            {
                var hits = Physics2D.RaycastAll(CameraMove.pointerPosition, Vector2.zero);
                bool contains = false;
                foreach (var hit in hits)
                    if (hit.transform == transform) contains = true;
                return contains && SelectionMove.enabled;
            }
        }

        bool StateChangedTo(CameraMoveState state)
        {
            return prevState != state && CameraMove.state == state;
        }

        bool StateChangedFrom(CameraMoveState state)
        {
            return prevState == state && CameraMove.state != state;
        }
    }

    [Serializable]
    public struct SelectionHandleState
    {
        public SelectionHandlerPhase phase;
        public float2 position;
        public float2 startPosition;
    }

    public enum SelectionHandlerPhase
    {
        Begin, Moved, End
    }
}