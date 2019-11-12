using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using VoyagerApp.Utilities;
using VoyagerApp.Workspace;
using VoyagerApp.Workspace.Views;

namespace VoyagerApp.UI
{
    public class BoxSelection : MonoBehaviour
    {
        [SerializeField] Transform selectionBox = null;

        CameraMoveState prevState;
        Transform currentSelection;
        Vector2 startPoint;

        List<LampItemView> lampOrder = new List<LampItemView>();

        void Update() => UpdateSelectState();

        void UpdateSelectState()
        {
            if ((StateChangedTo(CameraMoveState.WorkspaceClear) ||
                StateChangedTo(CameraMoveState.WorkspaceOverItem)) &&
                !CameraMove.Used)
            {
                RaycastHit2D hit = Physics2D.Raycast(CameraMove.pointerPosition, Vector3.zero);
                if (hit.transform == null)
                    OnSelectionStarted();
                else
                {
                    if (hit.transform.GetComponent<SelectionHandle>() == null)
                        OnSelectionStarted();
                }
            }

            if (currentSelection != null)
                UpdateBoxSelection();

            if (CameraMove.state == CameraMoveState.None && currentSelection != null)
                OnSelectionEnded();

            prevState = CameraMove.state;
        }

        void OnSelectionStarted()
        {
            currentSelection = Instantiate(selectionBox);
            startPoint = CameraMove.pointerPosition;
            OrderStart();
        }

        void UpdateBoxSelection()
        {
            var mid = (startPoint + CameraMove.pointerPosition) / 2.0f;
            currentSelection.position = new Vector3(mid.x, mid.y, -1.0f);

            var rawSize = CameraMove.pointerPosition - startPoint;
            var size = new Vector2(Mathf.Abs(rawSize.x), Mathf.Abs(rawSize.y));
            currentSelection.localScale = new Vector3(size.x, size.y, 1.0f);

            UpdateLampsOrder();
        }

        void OnSelectionEnded()
        {
            if (math.distance(startPoint, CameraMove.pointerPosition) < 0.05f)
                CheckIfClick();
            else
                ApplySelectionToItems(ItemsUnderSelection);

            Destroy(currentSelection.gameObject);
            currentSelection = null;

            SetItemOrders();
        }

        void CheckIfClick()
        {
            WorkspaceSelection.instance.Clear();

            if (WorkspaceManager.instance.GetItemsOfType<SelectionControllerView>().Length > 0)
            {
                Bounds bounds = SelectionController.bounds;
                bounds.Expand(new float3(float2.zero, 100.0f));

                if (bounds.Contains(CameraMove.pointerPosition))
                    OnItemClicked();
            }
            else OnItemClicked();
        }

        void OnItemClicked()
        {
            var hit = Physics2D.Raycast(CameraMove.pointerPosition, Vector3.zero);
            if (hit.transform != null)
            {
                WorkspaceItemView view = hit.transform.GetComponentInParent<WorkspaceItemView>(); 
                if (view is ISelectableItem selectable)
                    ApplySelectionToItems(new List<ISelectableItem> { selectable });
            }
        }

        void ApplySelectionToItems(List<ISelectableItem> items)
        {
            var selection = WorkspaceSelection.instance;

            switch (ApplicationState.SelectionMode.value)
            {
                case SelectionState.Set:
                    WorkspaceSelection.instance.Clear();
                    foreach (var item in items)
                        WorkspaceSelection.instance.SelectItem(item);
                    break;
                case SelectionState.Add:
                    foreach (var item in items)
                    {
                        if (!selection.Selected.Contains(item))
                            selection.SelectItem(item);
                    }
                    break;
                case SelectionState.Remove:
                    foreach (var item in items)
                    {
                        if (selection.Selected.Contains(item))
                            selection.DeselectItem(item);
                    }
                    break;
            }
        }

        bool StateChangedTo(CameraMoveState state)
        {
            return prevState != state && CameraMove.state == state;
        }

        #region Ordering

        List<LampItemView> lampOrderTemp;

        void OrderStart()
        {
            lampOrder.Clear();

            if (ApplicationState.SelectionMode.value != SelectionState.Set)
            {
                WorkspaceUtils.SelectedLampItemsInOrder.ForEach(lampOrder.Add);
                if (ApplicationState.SelectionMode.value == SelectionState.Remove)
                    lampOrderTemp = new List<LampItemView>(WorkspaceUtils.SelectedLampItemsInOrder);
            }
        }

        void UpdateLampsOrder()
        {
            var items = ItemsUnderSelection;
            var mode = ApplicationState.SelectionMode.value;

            if(mode == SelectionState.Remove)
            {
                lampOrder.Clear();
                foreach (var lamp in lampOrderTemp)
                {
                    if (!items.Contains(lamp))
                        lampOrder.Add(lamp);
                }
            }
            else
            {
                foreach (var item in items.ToArray())
                {
                    if (item is LampItemView lampItem)
                    {
                        if (mode == SelectionState.Add || mode == SelectionState.Set)
                            if (!lampOrder.Contains(lampItem))
                                lampOrder.Add(lampItem);
                    }
                }

                foreach (var lamp in lampOrder.ToArray())
                {
                    if (mode == SelectionState.Set)
                        if (!items.Contains(lamp) && lampOrder.Contains(lamp))
                            lampOrder.Remove(lamp);
                }
            }

            SetItemOrders();
        }

        void SetItemOrders()
        {
            foreach (var lamp in WorkspaceUtils.LampItems)
            {
                if (lampOrder.Contains(lamp))
                    lamp.Order = lampOrder.IndexOf(lamp) + 1;
                else
                    lamp.Order = -1;
            }
        }
        #endregion

        List<ISelectableItem> ItemsUnderSelection
        {
            get
            {
                Bounds bounds = currentSelection.GetComponent<BoxCollider2D>().bounds;
                bounds.Expand(Vector3.forward * 100.0f);

                List<ISelectableItem> items = new List<ISelectableItem>();

                foreach (var item in WorkspaceUtils.SelectableItems)
                {
                    if (item.SelectPositions.Any(p => bounds.Contains(p)))
                        items.Add(item);
                }

                return items;
            }
        }
    }
}
