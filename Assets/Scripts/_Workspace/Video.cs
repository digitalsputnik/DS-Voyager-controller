using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Video;
using Voyager.Workspace;

public class Video : MonoBehaviour {

	[Space(5)]
	[SerializeField] Collider col;
	[SerializeField] VideoPlayer videoPlayer;
	[SerializeField] Transform controller;
	public string videoUrl;
	public string filename;
	public Color outlineColor;

	WorkspaceItem item;
    LampMove movingLamp;
    GameObject outline;

    public void Setup(string url)
	{
		videoUrl = url;
		videoPlayer.url = videoUrl;
		videoPlayer.Prepare();
		videoPlayer.frame = 0;
		filename = Path.GetFileName(url).Split('.')[0];
		//videoPlayer.prepareCompleted += VideoPlayer_PrepareCompleted;
        
		item = GetComponent<WorkspaceItem>();

		StartCoroutine(SetupSize());
    }

    IEnumerator SetupSize()
	{
		yield return new WaitUntil(() => videoPlayer.isPrepared);

		LampMove move = GetComponent<LampMove>();
		int width = videoPlayer.texture.width;
        int height = videoPlayer.texture.height;
        float aspect = height / (float)width;
        Transform image = move.lampGraphics.GetChild(0);
        image.localScale = new Vector3(image.localScale.x, 30 * aspect, 1.0f);

        BoxCollider[] colliders = move.lampGraphics.GetComponents<BoxCollider>();
        foreach (BoxCollider coll in colliders)
            coll.size = new Vector3(coll.size.x, 30 * aspect, coll.size.z);

		outline = move.lampGraphics.GetChild(0).GetChild(3).gameObject;
        Vector3 outlineScale = Vector3.one;
        outlineScale.x += 0.02f;
        outlineScale.y += 0.02f / aspect;
        outline.transform.localScale = outlineScale;
        outline.GetComponent<MeshRenderer>().material.color = outlineColor;
		outline.SetActive(false);

		controller.localPosition = new Vector3(15.0f, -(30 * aspect) / 2.0f - 1.5f, 0.2f);
	}
    
	public void Play()
	{
		videoPlayer.Play();
	}

    public void Pause()
	{
		videoPlayer.Pause();
	}

    public void SetTime(float value)
	{
		videoPlayer.time = value;
	}
   
    public float GetVideoTime()
	{
		return (float)videoPlayer.time;
	}

    public float GetVideoLenght()
	{
		return videoPlayer.frameCount / videoPlayer.frameRate;
	}

	public VideoPlayer GetVideoPlayer()
	{
		return videoPlayer;
	}

	void Update()
	{
		CheckForMovingLamps();
	}

	void CheckForMovingLamps()
    {
        foreach (WorkspaceItem wi in Workspace.GetItemsInWorkspace())
        {
            if (wi.Type == WorkspaceItem.WorkspaceItemType.Lamp)
            {
                LampMove move = wi.GetComponent<LampMove>();
                if (move.moving)
                {
                    movingLamp = move;
                    DrawOutline();
                    return;
                }
            }
        }

        if (movingLamp != null)
        {
            MovingEnded();
            outline.SetActive(false);
            movingLamp = null;
        }
    }

	void DrawOutline()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(ray, out hit, 1000);
        col.enabled = true;
        WorkspaceItem workspaceItem = movingLamp.GetComponent<WorkspaceItem>();
        outline.SetActive(col.bounds.Contains(hit.point));
        col.enabled = false;
    }

	void MovingEnded()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(ray, out hit, 1000);
        col.enabled = true;
        WorkspaceItem workspaceItem = movingLamp.GetComponent<WorkspaceItem>();
        if (col.bounds.Contains(hit.point)) workspaceItem.SetParent(item);
        else
        {
            if (workspaceItem.parent == item)
                workspaceItem.SetParent(null);
        }
        col.enabled = false;
    }
}