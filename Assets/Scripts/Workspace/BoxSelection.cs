using System.Collections.Generic;
using UnityEngine;
using VoyagerApp.Utilities;
using VoyagerApp.Workspace;
using VoyagerApp.Workspace.Views;

namespace VoyagerApp.UI
{
    public class BoxSelection : MonoBehaviour
    {
        public static BoxSelectionMode mode = BoxSelectionMode.Set;

        [SerializeField] Transform selectionBox = null;
         
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
            if (!Application.isMobilePlatform)
                SetModeOnDesktop();

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

            var selection = WorkspaceSelection.instance;

            switch (mode)
            {
                case BoxSelectionMode.Add:
                    foreach (var lamp in lamps)
                    {
                        if (!selection.Selected.Contains(lamp))
                            selection.SelectLamp(lamp);
                    }
                    break;
                case BoxSelectionMode.Remove:
                    foreach (var lamp in lamps)
                    {
                        if (selection.Selected.Contains(lamp))
                            selection.DeselectLamp(lamp);
                    }
                    break;
                case BoxSelectionMode.Set:
                    WorkspaceSelection.instance.Clear();
                    foreach (var lamp in lamps)
                        WorkspaceSelection.instance.SelectLamp(lamp);
                    break;
            }
        }

        void SetModeOnDesktop()
        {
            if (Input.GetKey(KeyCode.LeftShift))
                mode = BoxSelectionMode.Add;
            else if (Input.GetKey(KeyCode.LeftControl))
                mode = BoxSelectionMode.Remove;
            else
                mode = BoxSelectionMode.Set;
        }
    }

    public enum BoxSelectionMode
    {
        Add,
        Remove,
        Set
    }
}
