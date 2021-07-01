using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VoyagerController.Mapping;
using VoyagerController.ProjectManagement;
using VoyagerController.Workspace;

namespace VoyagerController.UI
{
    public class RemoveBin : MonoBehaviour
    {
        [SerializeField] private Image _image = null;
        [SerializeField] private Color _activeColor = Color.black;
        [SerializeField] private Color _normalColor = Color.black;

        private void Start()
        {
            SelectionMove.SelectionMoveStarted += SelectionMoveStarted;
            SelectionMove.SelectionMoveEnded += SelectionMoveEnded;
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            SelectionMove.SelectionMoveStarted -= SelectionMoveStarted;
            SelectionMove.SelectionMoveEnded -= SelectionMoveEnded;
        }

        private void SelectionMoveStarted() => gameObject.SetActive(!EffectMapper.EffectMappingIsActive);

        private void SelectionMoveEnded()
        {
            if (ItemOverBin(Input.mousePosition))
            {
                foreach (var selected in new List<WorkspaceItem>(WorkspaceSelection.GetSelected()))
                {
                    WorkspaceManager.RemoveItem(selected);
                    WorkspaceSelection.Clear();
                }
            }

            Project.AutoSave();
            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (ItemOverBin(Input.mousePosition))
                SetColorActive();
            else
                SetColorNormal();
        }

        private bool ItemOverBin(Vector2 position)
        {
            return RectTransformUtility.RectangleContainsScreenPoint(_image.rectTransform, position);
        }

        private void SetColorActive() => _image.color = _activeColor;
        private void SetColorNormal() => _image.color = _normalColor;
    }
}