using UnityEngine;

public class PlatformSpecific : MonoBehaviour
{
	[SerializeField] bool android = true;
	[SerializeField] bool ios = true;
	[SerializeField] bool desktop = true;

    void Start()
    {
        if (Application.platform == RuntimePlatform.Android && !android)
            gameObject.SetActive(false);

        if (Application.platform == RuntimePlatform.IPhonePlayer && !ios)
            gameObject.SetActive(false);

        if (!Application.isMobilePlatform && !desktop)
            gameObject.SetActive(false);
    }
}