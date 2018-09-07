using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinMaxValues : MonoBehaviour {

	public int minValue;
	public int maxValue;
	public int startValue;

    void Start()
    {
        gameObject.GetComponent<InputField>().text = startValue.ToString();
    }

    public void EndEdit()
	{
		if (gameObject.GetComponent<InputField>().isFocused)
			return;
		
		if (Convert.ToInt32(gameObject.GetComponent<InputField>().text) <= minValue)
			gameObject.GetComponent<InputField>().text = minValue.ToString();
        else if (Convert.ToInt32(gameObject.GetComponent<InputField>().text) >= maxValue)
			gameObject.GetComponent<InputField>().text = maxValue.ToString();
	}   
}