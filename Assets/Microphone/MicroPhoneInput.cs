using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

//[RequireComponent(typeof(AudioSource))]
//public class MicroPhoneInput : MonoBehaviour {
//    public float minThreshold = 0;
//    public float frequency = 0.0f;
//    public int audioSampleRate = 44100;
//    public string microphone;
//    public FFTWindow fftWindow;
//    public Dropdown micDropdown;
//    public Slider thresholdSlider;

//    private List<string> options = new List<string>();
//    private int samples = 8192;
//    private AudioSource audioSource;

//    // Use this for initialization
//    void Start () {
//        //get components you'll need
//        audioSource = GetComponent<AudioSource>();

//        // get all available microphones
//        foreach (string device in Microphone.devices)
//        {
//            if (microphone == null)
//            {
//                //set default mic to first mic found.
//                microphone = device;
//            }
//            //Omitted due to not having a settings menu
//            //options.Add(device);
//        }
        
//        //Omitted due to not having a settings menu
//        //microphone = options[PlayerPrefsManager.GetMicrophone()];
//        //minThreshold = PlayerPrefsManager.GetThreshold();

//        ////add mics to dropdown
//        //micDropdown.AddOptions(options);
//        //micDropdown.onValueChanged.AddListener(delegate {
//        //    micDropdownValueChangedHandler(micDropdown);
//        //});

//        //thresholdSlider.onValueChanged.AddListener(delegate {
//        //    thresholdValueChangedHandler(thresholdSlider);
//        //});
//        ////initialize input with default mic
//        UpdateMicrophone();
//    }
	
//	// Update is called once per frame
//	void UpdateMicrophone () {
//        audioSource.Stop();
//        //Start recording to audioclip from the mic
//        //audioSource.clip = Microphone.Start(microphone, true, 10, audioSampleRate);
//        audioSource.clip = Microphone.Start(microphone, true, 10, audioSampleRate);
//        audioSource.loop = true;
//        // Mute the sound with an Audio Mixer group becuase we don't want the player to hear it
//        Debug.Log(Microphone.IsRecording(microphone).ToString());

//        if (Microphone.IsRecording(microphone))
//        { //check that the mic is recording, otherwise you'll get stuck in an infinite loop waiting for it to start
//            while (!(Microphone.GetPosition(microphone) > 0))
//            {
//            } // Wait until the recording has started. 

//            Debug.Log("recording started with " + microphone);

//            // Start playing the audio source
//            audioSource.Play();
//        }
//        else
//        {
//            //microphone doesn't work for some reason

//            Debug.Log(microphone + " doesn't work!");
//        }
//    }
//}

[RequireComponent(typeof(AudioSource))]
public class MicroPhoneInput : MonoBehaviour
{

    //Written in part by Benjamin Outram

    //option to toggle the microphone listenter on startup or not
    public bool startMicOnStartup = true;

    //allows start and stop of listener at run time within the unity editor
    public bool stopMicrophoneListener = false;
    public bool startMicrophoneListener = false;

    private bool microphoneListenerOn = false;

    //public to allow temporary listening over the speakers if you want of the mic output
    //but internally it toggles the output sound to the speakers of the audiosource depending
    //on if the microphone listener is on or off
    public bool disableOutputSound = false; 
 
     //an audio source also attached to the same object as this script is
     AudioSource src;

    //make an audio mixer from the "create" menu, then drag it into the public field on this script.
    //double click the audio mixer and next to the "groups" section, click the "+" icon to add a 
    //child to the master group, rename it to "microphone".  Then in the audio source, in the "output" option, 
    //select this child of the master you have just created.
    //go back to the audiomixer inspector window, and click the "microphone" you just created, then in the 
    //inspector window, right click "Volume" and select "Expose Volume (of Microphone)" to script,
    //then back in the audiomixer window, in the corner click "Exposed Parameters", click on the "MyExposedParameter"
    //and rename it to "Volume"
    public AudioMixer masterMixer;


    float timeSinceRestart = 0;






    void Start()
    {
        //start the microphone listener
        if (startMicOnStartup)
        {
            RestartMicrophoneListener();
            StartMicrophoneListener();
        }
    }

    void Update()
    {

        //can use these variables that appear in the inspector, or can call the public functions directly from other scripts
        if (stopMicrophoneListener)
        {
            StopMicrophoneListener();
        }
        if (startMicrophoneListener)
        {
            StartMicrophoneListener();
        }
        //reset paramters to false because only want to execute once
        stopMicrophoneListener = false;
        startMicrophoneListener = false;

        //must run in update otherwise it doesnt seem to work
        MicrophoneIntoAudioSource(microphoneListenerOn);

        //can choose to unmute sound from inspector if desired
        DisableSound(!disableOutputSound);


    }


    //stops everything and returns audioclip to null
    public void StopMicrophoneListener()
    {
        //stop the microphone listener
        microphoneListenerOn = false;
        //reenable the master sound in mixer
        disableOutputSound = false;
        //remove mic from audiosource clip
        src.Stop();
        src.clip = null;

        Microphone.End(null);
    }


    public void StartMicrophoneListener()
    {
        //start the microphone listener
        microphoneListenerOn = true;
        //disable sound output (dont want to hear mic input on the output!)
        disableOutputSound = true;
        //reset the audiosource
        RestartMicrophoneListener();
    }


    //controls whether the volume is on or off, use "off" for mic input (dont want to hear your own voice input!) 
    //and "on" for music input
    public void DisableSound(bool SoundOn)
    {

        float volume = 0;

        if (SoundOn)
        {
            volume = 0.0f;
        }
        else
        {
            volume = -80.0f;
        }

        masterMixer.SetFloat("volume", volume);
    }



    // restart microphone removes the clip from the audiosource
    public void RestartMicrophoneListener()
    {

        src = GetComponent<AudioSource>();

        //remove any soundfile in the audiosource
        src.clip = null;

        timeSinceRestart = Time.time;

    }

    //puts the mic into the audiosource
    void MicrophoneIntoAudioSource(bool MicrophoneListenerOn)
    {

        if (MicrophoneListenerOn)
        {
            //pause a little before setting clip to avoid lag and bugginess
            if (Time.time - timeSinceRestart > 0.5f && !Microphone.IsRecording(null))
            {
                src.clip = Microphone.Start(null, true, 10, 44100);

                //wait until microphone position is found (?)
                while (!(Microphone.GetPosition(null) > 0))
                {
                }

                src.Play(); // Play the audio source
            }
        }
    }

}
