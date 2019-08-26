using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class JoystickHandle : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
	Vector2 offset;
	RectTransform rect;

	[SerializeField] Canvas canvas;

	void Start()
	{
		rect = GetComponent<RectTransform>();
        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		offset = eventData.position + rect.anchoredPosition * canvas.scaleFactor;
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