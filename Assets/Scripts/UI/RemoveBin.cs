using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VoyagerApp.Workspace;

namespace VoyagerApp.UI
{
    public class RemoveBin : MonoBehaviour
    {
        [SerializeField] Image image        = null;
        [SerializeField] Color activeColor  = Color.black;
        [SerializeField] Color normalColor  = Color.black;

        List<ItemMove> itemsMoving = new List<ItemMove>();

        void Start()
        {
            ItemMove.onItemMoveStarted += ItemMoveStarted;
            ItemMove.onItemMoveEnded += ItemMoveEnded;

            gameObject.SetActive(false);
        }

        private void ItemMoveStarted(ItemMove item)
        {
            itemsMoving.Add(item);
            gameObject.SetActive(true);
        }

        private void ItemMoveEnded(ItemMove item)
        {
            itemsMoving.Remove(item);
            
            if (ItemOverBin(Input.mousePosition))
            {
                var view = item.GetComponentInParent<WorkspaceItemView>();
                WorkspaceManager.instance.RemoveItem(view);
            }

            if (itemsMoving.Count == 0)
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
            ItemMove.onItemMoveStarted -= ItemMoveStarted;
            ItemMove.onItemMoveEnded -= ItemMoveEnded;
        }

        void SetColorActive() => image.color = activeColor;
        void SetColorNormal() => image.color = normalColor;
    }
}