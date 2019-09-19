using UnityEngine;
using VoyagerApp.Workspace;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MoveIcon : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public static bool pressed;

    [SerializeField] Color pressedColor = Color.white;
    [SerializeField] Color releasedColor = Color.white;

    Image image;

    void Start()
    {
        if (Application.isMobilePlatform)
        {
            image = GetComponent<Image>();
            image.color = releasedColor;

            ItemMove.onItemMoveStarted += ItemMoveStarted;
            ItemMove.onItemMoveEnded += ItemMoveEnded;
        }
        else
            gameObject.SetActive(false);
    }

    private void ItemMoveStarted(ItemMove item)
    {
        gameObject.SetActive(false);
    }

    private void ItemMoveEnded(ItemMove item)
    {
        gameObject.SetActive(true);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        image.color = pressedColor;
        pressed = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        image.color = releasedColor;
        pressed = false;
    }
}