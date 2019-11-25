using UnityEngine;
using UnityEngine.EventSystems;

public class JoystickHandle : MonoBehaviour, IDragHandler, IEndDragHandler, IPointerDownHandler
{
	Vector2 offset;
	RectTransform rect;

	[SerializeField] Canvas canvas;
    bool dragging;

	void Start()
	{
		rect = GetComponent<RectTransform>();
        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();
	}

    public void OnPointerDown(PointerEventData eventData)
    {
        offset = eventData.position;
        dragging = true;
    }

	public void OnDrag(PointerEventData eventData)
	{
		rect.anchoredPosition = (eventData.position - offset) / canvas.scaleFactor;
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		rect.anchoredPosition = Vector2.zero;
	}
}