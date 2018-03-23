using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeIn : MonoBehaviour {

	void Start()
	{
		StartCoroutine(FadeTextToFullAlpha(2f, GetComponent<Text>()));
	}



	public IEnumerator FadeTextToFullAlpha(float delay, Text text)
	{
		text.color = new Color(text.color.r, text.color.g, text.color.b, 0);
		while (text.color.a < 1.0f)
		{
			text.color = new Color(text.color.r, text.color.g, text.color.b, text.color.a + (Time.deltaTime / delay));
			yield return null;
		}
	}
		
}
