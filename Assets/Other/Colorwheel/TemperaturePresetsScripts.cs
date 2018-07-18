using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TemperaturePresetsScripts : MonoBehaviour {

    public Slider ControlledSlider;
    public Button Button3200 { get; set; }
    public Button Button5600 { get; set; }

    // Use this for initialization
    void Start()
    {
        //Get buttons
        Button3200 = transform.Find("Button3200").GetComponent<Button>();
        Button5600 = transform.Find("Button5600").GetComponent<Button>();

        //Add listeners to buttons
        Button3200.onClick.AddListener(OnPreset3200Click);
        Button5600.onClick.AddListener(OnPreset5600Click);
    }

    private void OnPreset3200Click()
    {
        ControlledSlider.value = 3200;
    }

    private void OnPreset5600Click()
    {
        ControlledSlider.value = 5600;
    }
    
}
