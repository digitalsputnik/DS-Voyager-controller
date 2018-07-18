using UnityEngine;
using UnityEngine.SceneManagement;
using Voyager.Lamps;

public class LampDetectionMenu : MonoBehaviour {
    
    public void BackBtn()
	{
		PlayerPrefs.SetInt("ComingFromDetectionScene", 1);
		SceneManager.LoadScene(0);
	}

    public void AddDetectedLampsBtn()
	{
		PlayerPrefs.SetInt("ComingFromDetectionScene", 2);
		GameObject.FindWithTag("LampManager").GetComponent<LampManager>().SaveWorkplace("detection");      
		SceneManager.LoadScene(0);
	}
}