using UnityEngine;

public class Joystick : MonoBehaviour
{
	public float AreaRadius = 50.0f;
	public float Horizontal { get; private set; }
	public float Vertical { get; private set; }
	
	Transform handle;
	RectTransform handleRect;

	void Start()
	{
		handle = transform.GetChild(0);
		handleRect = handle.GetComponent<RectTransform>();
	}

	void Update()
	{
		Vector2 curPos = handleRect.anchoredPosition;
		float magnitude = Vector2.Distance(curPos, Vector2.zero);

		if(magnitude > AreaRadius)
		{
			curPos.x *= AreaRadius / magnitude;
			curPos.y *= AreaRadius / magnitude;
			handleRect.anchoredPosition = curPos;
		}

		Horizontal = curPos.x / AreaRadius;
		Vertical   = curPos.y / AreaRadius;
	}
}