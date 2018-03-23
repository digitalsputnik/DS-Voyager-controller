using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AudioVisualizer : MonoBehaviour {

    public Transform[] audioSpectrumObjects;
    //[Range(1, 100)] public float heightMultiplier;
    public FFTWindow fftWindow;
    //public Slider sensitivitySlider;
    public AnimationLayer anim;
    [Range(64, 8192)] public int numberOfSamples = 512; //step by 2 //NOTE: is this too slow!?
    //public float lerpTime = 1;
    [Range(0.0f, 1.0f)] public float BaseIntensity = 0.001f;
    private float SensitivityFactor = 0.05f;
    //[Range(0.0f, 1.0f)] public float BPMspeed; //NOTE: 1.0 is equivalent to 0.0!

    // Use this for initialization
    void Start () {
        BeatDetection processor = FindObjectOfType<BeatDetection>();
        processor.onBeat.AddListener(onOnbeatDetected);
        //processor.onSpectrum.AddListener(onSpectrum);
    }

    void onOnbeatDetected()
    {
        //Increment hue if mic!
        if (anim.specialPlaybackMode == specialMode.mic)
        {
            //anim.BPMincrement = anim.BPMincrement + (float)anim.draw.anim.secI / 200.0f;
        }
        //Protection for not overspinning
        if (anim.BPMincrement > 2.0f)
            anim.BPMincrement = anim.BPMincrement % 2.0f;
        print("Beat!!!");
    }

    //This event will be called every frame while music is playing
    //void onSpectrum(float[] spectrum)
    //{
    //    //The spectrum is logarithmically averaged
    //    //to 12 bands

    //    for (int i = 0; i < spectrum.Length; ++i)
    //    {
    //        Vector3 start = new Vector3(i, 0, 0);
    //        Vector3 end = new Vector3(i, spectrum[i], 0);
    //        Debug.DrawLine(start, end);
    //    }
    //}

    // Update is called once per frame
    void Update () {
        //Get audiosource
        AudioSource audioSource = GetComponent<AudioSource>();
        
        // populate array with fequency spectrum data
        float[] spectrum = new float[numberOfSamples];
        audioSource.GetSpectrumData(spectrum, 0, fftWindow);
        //SensitivityFactor = (float)anim.draw.anim.main.iVal * 0.3f;
        anim.audioIntensity = spectrum.Select(x => x * SensitivityFactor + BaseIntensity).ToArray();
    }
}
