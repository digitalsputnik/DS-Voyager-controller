using System.Collections.Generic;
using UnityEngine;
using VoyagerApp.Utilities;
using VoyagerApp.Workspace;
using VoyagerApp.Workspace.Views;

namespace VoyagerApp.UI
{
    public class BoxSelection : MonoBehaviour
    {
        [SerializeField] Transform selectionBox;

        CameraMoveState prevState;
        Transform currentSelection;
        Vector2 startPoint;

        void Update()
        {
            if (prevState != CameraMoveState.WorkspaceClear &&
                CameraMove.state == CameraMoveState.WorkspaceClear)
                OnSelectionStarted();

            if (prevState == CameraMoveState.WorkspaceClear &&
                CameraMove.state != CameraMoveState.WorkspaceClear)
                OnSelectionEnded();

            if (currentSelection != null)
                UpdateBoxSelection();

            prevState = CameraMove.state;
        }

        void OnSelectionStarted()
        {
            currentSelection = Instantiate(selectionBox);
            startPoint = CameraMove.pointerPosition;
        }

        void UpdateBoxSelection()
        {
            var mid = (startPoint + CameraMove.pointerPosition) / 2.0f;
            currentSelection.position = new Vector3(mid.x, mid.y, -1.0f);

            var rawSize = CameraMove.pointerPosition - startPoint;
            var size = new Vector2(Mathf.Abs(rawSize.x), Mathf.Abs(rawSize.y));
            currentSelection.localScale = new Vector3(size.x, size.y, 1.0f);
        }

        void OnSelectionEnded()
        {
            CheckForItems();
            Destroy(currentSelection.gameObject);
            currentSelection = null;
        }

        void CheckForItems()
        {
            Bounds bounds = currentSelection.GetComponent<BoxCollider2D>().bounds;
            bounds.Expand(Vector3.forward * 10.0f);

            List<LampItemView> lamps = new List<LampItemView>();

            foreach (var item in WorkspaceUtils.LampItems)
            {
                if (bounds.Contains(item.transform.position))
                    lamps.Add(item);
            }

            if (lamps.Count > 0)
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    foreach (var lamp in lamps)
                    {
                        if (!WorkspaceSelection.instance.Selected.Contains(lamp))
                            WorkspaceSelection.instance.SelectLamp(lamp);
                    }
                }
                else
                {
                    WorkspaceSelection.instance.Clear();
                    foreach (var lamp in lamps)
                        WorkspaceSelection.instance.SelectLamp(lamp);
                }
            }
        }
    }
}