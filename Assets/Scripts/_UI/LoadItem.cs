using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;
using Voyager.Lamps;
using Voyager.Workspace;

public class LoadItem : MonoBehaviour {
    
	[SerializeField] Text nameText;
	[SerializeField] Text dateText;
	[SerializeField] Text lampsText;
	[SerializeField] Text imagesText;
	[SerializeField] RawImage thumbnail;

	string workspaceName;
	string path;
	LoadPanel panel;
    
	public void Setup (string path, LoadPanel panel)
	{
		this.panel = panel;
		this.path = path;
		workspaceName = Path.GetFileName(path).Split('.')[0];

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(path, FileMode.Open);
        Workspace.WorkplaceData workspace = (Workspace.WorkplaceData)bf.Deserialize(file);
        file.Close();
        file.Dispose();


		List<Lamp> lamps = new List<Lamp>();

		foreach(Lamp lamp in LampManager.Instance.GetLamps())
		{
			foreach(LampSaveData lampData in workspace.lamps)
			{
				if (lamp.Serial == lampData.serial && !lamp.ConnectionLost)
					lamps.Add(lamp);
			}
		}

		if (workspace.thumbnail != "")
			SetThumbnail(workspace.thumbnail);

		nameText.text = workspaceName;
		dateText.text = "Created " + File.GetCreationTime(path);
		lampsText.text = "Lamp count: " + workspace.lamps.Length + " - found " + lamps.Count;
		imagesText.text = "Image count: " + workspace.images.Length;
	}

	void SetThumbnail(string thumbnailPath)
	{
		byte[] file = File.ReadAllBytes(thumbnailPath);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(file);
		thumbnail.texture = texture;
		thumbnail.color = Color.white;
	}

    public void Load()
	{
		Workspace.Clear();
		Workspace.LoadWorkplace(workspaceName);
		panel.savePanel.nameField.text = workspaceName;
        panel.Close();
    }

    public void Delete()
    {
        File.Delete(path);
		Destroy(gameObject);
	}
}
