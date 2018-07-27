using UnityEngine;
using UnityEngine.SceneManagement;
using Voyager.Lamps;
using Voyager.Workspace;

public class LampDetectionMenu : MonoBehaviour {
    
    public void BackBtn()
	{
		PlayerPrefs.SetInt("ComingFromDetectionScene", 1);
		SceneManager.LoadScene(0);
	}
    
    public void AddDetectedLampsBtn()
	{
		PlayerPrefs.SetInt("ComingFromDetectionScene", 2);
		Workspace.SaveWorkplace("detection");      
		SceneManager.LoadScene(0);
	}
}