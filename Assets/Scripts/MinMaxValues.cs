using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinMaxValues : MonoBehaviour {

	public int minValue;
	public int maxValue;
	public int startValue;

    private void Start()
    {
        this.gameObject.GetComponent<InputField>().text = startValue.ToString();
    }

    private void Update()
    {
        if (Convert.ToInt32(this.gameObject.GetComponent<InputField>().text) <= minValue)
        {
            this.gameObject.GetComponent<InputField>().text = minValue.ToString();
        }
        else if (Convert.ToInt32(this.gameObject.GetComponent<InputField>().text) >= maxValue) {
            this.gameObject.GetComponent<InputField>().text = maxValue.ToString();
        }
    }


}
