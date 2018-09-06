using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour 
{
	CanvasGroup canvasGroup;

	void Start()
	{
		canvasGroup = GetComponent<CanvasGroup>();
		Hide();

		RectTransform rect = GetComponent<RectTransform>();
		rect.offsetMin = Vector2.zero;
		rect.offsetMax = Vector2.zero;
	}

	public void Show()
	{
		canvasGroup.interactable = true;
		canvasGroup.blocksRaycasts = true;
		canvasGroup.alpha = 1.0f;
	}

    public void Hide()
	{
		canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.0f;
	}
}