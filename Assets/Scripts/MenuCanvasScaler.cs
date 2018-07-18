#if (UNITY_ANDROID || UNITY_IOS)
#define MOBILE
#endif

using UnityEngine;
using UnityEngine.UI;

public class MenuCanvasScaler : MonoBehaviour
{
	[SerializeField] Slider scaleFactorSlider;
	[SerializeField] float minScale, maxScale; // 1, 2
	[SerializeField] RectTransform menuObject;
	[SerializeField] MenuPullScript menuPull;
	CanvasScaler scaler;

	void Start()
	{
		scaler = GetComponent<CanvasScaler>();
		SetCanvasScaleMode();
		if(PlayerPrefs.HasKey("UI_scale_factor"))
		    scaleFactorSlider.value = PlayerPrefs.GetFloat("UI_scale_factor");
		OnScaleFactorChanged();
	}

	void SetCanvasScaleMode()
	{      
		if (Application.platform == RuntimePlatform.Android ||
		    Application.platform == RuntimePlatform.IPhonePlayer
		   )
		{
			scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaleFactorSlider.gameObject.SetActive(false);
			menuPull.transform.parent.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
			menuObject.anchoredPosition = new Vector2(75f, -240f);
		}
		else
		{
			scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            scaleFactorSlider.gameObject.SetActive(true);
			menuPull.transform.parent.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1.0f);
			menuObject.anchoredPosition = new Vector2(75f, 0);
		}

		menuPull.CalculatePositions();
	}

    public void OnScaleFactorChanged()
	{
		scaler.scaleFactor = (maxScale - minScale) * scaleFactorSlider.value + minScale;
		PlayerPrefs.SetFloat("UI_scale_factor", scaleFactorSlider.value);
	}
}