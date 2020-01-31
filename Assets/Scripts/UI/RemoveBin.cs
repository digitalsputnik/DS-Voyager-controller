using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VoyagerApp.Utilities;
using VoyagerApp.Workspace;
using VoyagerApp.Workspace.Views;

namespace VoyagerApp.UI
{
    public class RemoveBin : MonoBehaviour
    {
        [SerializeField] Image image        = null;
        [SerializeField] Color activeColor  = Color.black;
        [SerializeField] Color normalColor  = Color.black;

        void Start()
        {
            SelectionMove.onSelectionMoveStarted += SelectionMoveStarted;
            SelectionMove.onSelectionMoveEnded += SelectionMoveEnded;
            gameObject.SetActive(false);
        }

        private void SelectionMoveStarted()
        {
            gameObject.SetActive(true);
        }

        private void SelectionMoveEnded()
        {
            if (ItemOverBin(Input.mousePosition))
            {
                foreach (var selected in new List<ISelectableItem>(WorkspaceUtils.SelectedItems))
                    if (selected is WorkspaceItemView view)
                    {
                        if (view is LampItemView lampView)
                        {
                            lampView.lamp.buffer.Clear();
                            lampView.lamp.effect = null;
                        }

                        WorkspaceManager.instance.RemoveItem(view);
                    }
                WorkspaceSelection.instance.Clear();
            }
            gameObject.SetActive(false);
        }

        void Update()
        {
            if (ItemOverBin(Input.mousePosition))
                SetColorActive();
            else
                SetColorNormal();

         }

        bool ItemOverBin(Vector2 position)
        {
            return RectTransformUtility.RectangleContainsScreenPoint(image.rectTransform, position);
        }

        void OnDestroy()
        {
            SelectionMove.onSelectionMoveStarted -= SelectionMoveStarted;
            SelectionMove.onSelectionMoveEnded -= SelectionMoveEnded;
        }

        void SetColorActive() => image.color = activeColor;
        void SetColorNormal() => image.color = normalColor;
    }
}