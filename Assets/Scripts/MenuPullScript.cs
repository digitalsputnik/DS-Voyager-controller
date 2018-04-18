using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuPullScript : MonoBehaviour, IPointerClickHandler {

    public enum PullDirection
    {
        Up,
        Left
    }

    public GameObject Menu;
    public float TimeOfTravel = 0.2f;
    public PullDirection pulling = PullDirection.Left;
	public Sprite imageOne;
	public Sprite imageTwo;

    private bool onMove = false;
    private Vector3 StartPosition;
    private Vector3 EndPosition;
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


    // Use this for initialization
    void Start () {
        StartPosition = Menu.transform.position;
        EndPosition = StartPosition;
        var rt = (RectTransform)Menu.transform;

        if (pulling == PullDirection.Left)
            EndPosition.x = EndPosition.x - rt.rect.width * (Menu.transform.lossyScale.magnitude / Menu.transform.localScale.magnitude);
        else if (pulling == PullDirection.Up)
            EndPosition.y = EndPosition.y + rt.rect.height * (Menu.transform.lossyScale.magnitude / Menu.transform.localScale.magnitude);
    }
	
	// Update is called once per frame
	void Update () {
        if (onMove)
        {
            currentTime += Time.deltaTime;
            normalizedTime = currentTime / TimeOfTravel;
            Menu.transform.position = Vector3.Lerp(StartPosition, EndPosition, normalizedTime);
            if (currentTime >= TimeOfTravel)
            {
                onMove = false;
                EndPosition = StartPosition;
                StartPosition = Menu.transform.position;
            }
        }
	}

}
