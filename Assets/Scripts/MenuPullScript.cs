using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuPullScript : MonoBehaviour, IPointerClickHandler {

    public GameObject Menu;
    public float TimeOfTravel = 0.2f;
	public Sprite imageOne;
	public Sprite imageTwo;

	RectTransform rect;
	[SerializeField] Canvas canvas;

    private bool onMove = false;
    private Vector2 StartPosition;
	private Vector2 EndPosition;
    private float currentTime = 0.0f;
    private float normalizedTime = 0.0f;
	public bool isPulled = false;

    public void OnPointerClick(PointerEventData eventData)
    {
        MoveMenu();
    }

    public void MoveMenu()
    {
        onMove = true;
        currentTime = 0.0f;
        isPulled = !isPulled;
        Sprite newSprite;

        if (isPulled)
        {
            newSprite = imageTwo; // <- This is the new sprite
        }
        else
        {
            newSprite = imageOne;
        }
        Image theImage = gameObject.GetComponent<Image>();
        theImage.sprite = newSprite;

        Startup start = GameObject.Find("Main Camera").GetComponent<Startup>();

        if (start.tutorialMode)
        {
            start.action1 = true;
            start.tutorial.transform.Find("HelpWindow").GetComponent<RectTransform>().sizeDelta = new Vector2(550f, 340f);
            start.tutorial.transform.Find("HelpWindow").GetComponent<RectTransform>().localPosition = new Vector3(-50f, 0f, 0f);
        }
    }


    public void CloseMenu()
    {
        onMove = true;
        currentTime = 0.0f;
        isPulled = !isPulled;
        Sprite newSprite;

        if (isPulled)
        {
            newSprite = imageTwo; // <- This is the new sprite
        }
        else
        {
            newSprite = imageOne;
        }
        Image theImage = gameObject.GetComponent<Image>();
        theImage.sprite = newSprite;

    }

	// Use this for initialization   
	void Start()
	{
		rect = Menu.GetComponent<RectTransform>();
		CalculatePositions();
	}

    public void CalculatePositions()
	{      
		StartPosition = rect.anchoredPosition;
        EndPosition = StartPosition;

		Debug.Log(StartPosition);

		EndPosition.x = -EndPosition.x;
	}

	// Update is called once per frame
	void Update () {
        if (onMove)
        {
            currentTime += Time.deltaTime;
            normalizedTime = currentTime / TimeOfTravel;
			rect.anchoredPosition = Vector2.Lerp(StartPosition, EndPosition, normalizedTime);
            if (currentTime >= TimeOfTravel)
            {
                onMove = false;
                EndPosition = StartPosition;
				StartPosition = rect.anchoredPosition;
            }
        }
	}

}
