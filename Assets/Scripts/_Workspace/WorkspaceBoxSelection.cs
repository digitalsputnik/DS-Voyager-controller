using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace VoyagerController.Workspace
{
    public class WorkspaceBoxSelection : MonoBehaviour
    {
        [SerializeField] private Transform _selectionBox = null;

        private CameraMoveState _prevState;
        private Transform _currentSelection;
        private Vector2 _startPoint;

        private readonly List<VoyagerItem> _lampOrder = new List<VoyagerItem>();

        private void Update() => UpdateSelectState();

        private void UpdateSelectState()
        {
            if ((StateChangedTo(CameraMoveState.WorkspaceClear) ||
                StateChangedTo(CameraMoveState.WorkspaceOverItem)) &&
                !CameraMove.Used)
            {
                RaycastHit2D hit = Physics2D.Raycast(CameraMove.PointerPosition, Vector3.zero);
                if (hit.transform == null)
                    OnSelectionStarted();
                else
                {
                    if (hit.transform.GetComponent<SelectionHandle>() == null)
                        OnSelectionStarted();
                }
            }

            if (_currentSelection != null)
                UpdateBoxSelection();

            if (CameraMove.State == CameraMoveState.None && _currentSelection != null)
                OnSelectionEnded();

            _prevState = CameraMove.State;
        }

        private void OnSelectionStarted()
        {
            _currentSelection = Instantiate(_selectionBox);
            _startPoint = CameraMove.PointerPosition;
            OrderStart();
        }

        private void UpdateBoxSelection()
        {
            var mid = (_startPoint + CameraMove.PointerPosition) / 2.0f;
            _currentSelection.position = new Vector3(mid.x, mid.y, -1.0f);

            var rawSize = CameraMove.PointerPosition - _startPoint;
            var size = new Vector2(Mathf.Abs(rawSize.x), Mathf.Abs(rawSize.y));
            _currentSelection.localScale = new Vector3(size.x, size.y, 1.0f);

            UpdateLampsOrder();
        }

        private void OnSelectionEnded()
        {
            if (math.distance(_startPoint, CameraMove.PointerPosition) < 0.05f)
                OnItemClicked();
            else
                ApplySelectionToItems(ItemsUnderSelection);

            Destroy(_currentSelection.gameObject);
            _currentSelection = null;

            SetItemOrders();
        }

        private void OnItemClicked()
        {
            var hit = Physics2D.Raycast(CameraMove.PointerPosition, Vector3.zero);
            if (hit.transform != null)
            {
                var view = hit.transform.GetComponentInParent<WorkspaceItem>();
                
                if (view != null)
                {
                    if (view.Selectable)
                        ApplySelectionToItems(new List<WorkspaceItem> { view });
                    else
                        WorkspaceSelection.Clear();
                }
            }
            else
                WorkspaceSelection.Clear();
        }

        void ApplySelectionToItems(List<WorkspaceItem> items)
        {
            switch (ApplicationState.SelectMode.Value)
            {
                case SelectionMode.Set:
                    WorkspaceSelection.Clear();
                    foreach (var item in items)
                        WorkspaceSelection.SelectItem(item);
                    break;
                case SelectionMode.Add:
                    foreach (var item in items)
                    {
                        if (!WorkspaceSelection.Contains(item))
                            WorkspaceSelection.SelectItem(item);
                    }
                    break;
                case SelectionMode.Remove:
                    foreach (var item in items)
                    {
                        if (WorkspaceSelection.Contains(item))
                            WorkspaceSelection.DeselectItem(item);
                    }
                    break;
            }
        }

        private bool StateChangedTo(CameraMoveState state) => _prevState != state && CameraMove.State == state;

        #region Ordering
        private List<VoyagerItem> lampOrderTemp;

        private void OrderStart()
        {
            _lampOrder.Clear();

            if (ApplicationState.SelectMode.Value != SelectionMode.Set)
            {
                WorkspaceUtils.SelectedVoyagerItemsInOrder.ForEach(_lampOrder.Add);
                if (ApplicationState.SelectMode.Value == SelectionMode.Remove)
                    lampOrderTemp = new List<VoyagerItem>(WorkspaceUtils.SelectedVoyagerItemsInOrder);
            }
        }

        private void UpdateLampsOrder()
        {
            var items = ItemsUnderSelection;
            var mode = ApplicationState.SelectMode.Value;

            if (mode == SelectionMode.Remove)
            {
                _lampOrder.Clear();
                foreach (var lamp in lampOrderTemp)
                {
                    if (!items.Contains(lamp))
                        _lampOrder.Add(lamp);
                }
            }
            else
            {
                foreach (var item in items.ToArray())
                {
                    if (item is VoyagerItem voyager)
                    {
                        if (mode == SelectionMode.Add || mode == SelectionMode.Set)
                            if (!_lampOrder.Contains(voyager))
                                _lampOrder.Add(voyager);
                    }
                }

                foreach (var lamp in _lampOrder.ToArray())
                {
                    if (mode == SelectionMode.Set)
                        if (!items.Contains(lamp) && _lampOrder.Contains(lamp))
                            _lampOrder.Remove(lamp);
                }
            }

            SetItemOrders();
        }

        void SetItemOrders()
        {
            foreach (var lamp in WorkspaceManager.GetItems<VoyagerItem>())
            {
                if (_lampOrder.Contains(lamp))
                    lamp.Order = _lampOrder.IndexOf(lamp) + 1;
                else
                    lamp.Order = -1;
            }
        }
        #endregion

        private List<WorkspaceItem> ItemsUnderSelection
        {
            get
            {
                Bounds bounds = _currentSelection.GetComponent<BoxCollider2D>().bounds;
                bounds.Expand(Vector3.forward * 100.0f);

                var items = new List<WorkspaceItem>();

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
