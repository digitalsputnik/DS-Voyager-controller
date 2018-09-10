using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum playbackState {stop,play,pause}
public enum playbackMode {loop,hold}
public enum specialMode {normal,fire,police,gradient,mic,chaser}

public class AnimationLayer : MonoBehaviour {

	[Header("Controls")]
	public float currentTime;
	public float timelineLength;
	public float timeScale;
	public playbackState state;
	public playbackMode mode;
    public float[] audioIntensity;
    public float BPMincrement;
    [Range(0.0f, 1.0f)] public float RandomizerRange;
	[Space (10)]
	[Header("Curves")]
	public AnimationCurve ICurve;
	public float IPixelOffset;
	public AnimationCurve TCurve;
	public float TPixelOffset;
	public AnimationCurve SCurve;
	public float SPixelOffset;
	public AnimationCurve HCurve;
	public float HPixelOffset;
	[Space (10)]
	[Header("Tools")]
	public DrawMode draw;
    public tempAnimcontroller anim;
	public List<Pixel> controlledPixels;
    public List<Pixel> controlledPixelsOnTop;
    public List<float[]> randomizer = new List<float[]>();
	public specialMode specialPlaybackMode;
	public float flicker;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        //is stopped and time needs to be reset
		if (state == playbackState.stop && currentTime!=0) 
			currentTime = 0f;
		// is stopped
		if (state == playbackState.stop)
			return;
		// is paused
		if (state == playbackState.pause)
			return;
		//loop timeline back to 0
		if (mode==playbackMode.loop && timelineLength > 0f && currentTime > timelineLength)
			currentTime = 0f;
		//stop timeline in hold mode at the end
		if (mode==playbackMode.hold && state == playbackState.play && currentTime >= timelineLength) {
			currentTime = 0f;
			state = playbackState.stop;
			return;
		}

		currentTime = currentTime + (Time.deltaTime*1000*timeScale);
		flicker = UnityEngine.Random.Range(0.0f, 1.0f);
		//updatePixels ();
	}

	public void updatePixels() {
        //Temporary solution
        //UnityEngine.Random.InitState(42);
        for (int i=0;i<controlledPixels.Count;i++) {
            //Check if there are layers on top of this!
            if (controlledPixelsOnTop.Contains(controlledPixels[i]))
            {
                //UnityEngine.Random.Range((float)anim.secI / 100.0f, (float)anim.oldI / 100.0f);
                //UnityEngine.Random.Range((float)anim.secT, (float)anim.oldT);
                //UnityEngine.Random.Range((float)anim.secS / 100.0f, (float)anim.oldS / 100.0f);
                //UnityEngine.Random.Range((float)anim.secH / 360.0f, (float)anim.oldH / 360.0f);
                continue;
            }
            //TODO: Move to pixel adding, this is too slow!
            //int buttonIndex = draw.StrokeListMenu.childCount - 1;
            //tempAnimcontroller TopStrokeAnimation = draw.StrokeListMenu.GetChild(buttonIndex).GetComponent<StrokeOptionScripts>().Animation;
            //bool pixelControlled = false;
            //while (TopStrokeAnimation != anim && pixelControlled == false)
            //{
            //    if (TopStrokeAnimation == null)
            //        continue;

            //    if (TopStrokeAnimation.layer.controlledPixels.Contains(controlledPixels[i]))
            //    {
            //        pixelControlled = true;
            //    }
            //    buttonIndex--;
            //    TopStrokeAnimation = draw.StrokeListMenu.GetChild(buttonIndex).GetComponent<StrokeOptionScripts>().Animation;
            //}

            //if (pixelControlled)
            //    continue;
            //TODO: Move this up to this point!
            
            //set old pixel value
			draw.setITSH ((int)(controlledPixels[i].ITSH[0] * 100), (int)(controlledPixels[i].ITSH[1] * 10000), (int)(controlledPixels[i].ITSH[2] * 100), (int)(controlledPixels[i].ITSH[3] * 360));

            //set animated values
            if (specialPlaybackMode == specialMode.normal)
            {
                draw.setITSH((((float)anim.oldI)) / 100.0f, 0);
                draw.setITSH((((float)anim.oldT)), 1);
                draw.setITSH((((float)anim.oldS)) / 100.0f, 2);
                draw.setITSH((((float)anim.oldH)) / 360.0f, 3);
                //if (ICurve != null)
                //	draw.setITSH (ICurve.curveUpdate (currentTime + (i * IPixelOffset)), 0);

                //if (TCurve != null)
                //	draw.setITSH (TCurve.curveUpdate (currentTime + (i * TPixelOffset)), 1);

                //if (SCurve != null)
                //	draw.setITSH (SCurve.curveUpdate (currentTime + (i * SPixelOffset)), 2);

                //if (HCurve != null)
                //	draw.setITSH (HCurve.curveUpdate (currentTime + (i * HPixelOffset)), 3);

            }
            else if (specialPlaybackMode == specialMode.fire)
            {
                //Option for precise randomizer
                //float random = UnityEngine.Random.Range(0.0f, 1.0f);
                //float invRandom = 1.0f - random;
                //draw.setITSH((anim.oldI * random + anim.secI * invRandom) / 100.0f, 0);
                draw.setITSH(UnityEngine.Random.Range((float)anim.secI / 100.0f, (float)anim.oldI / 100.0f), 0);
                draw.setITSH(UnityEngine.Random.Range((float)anim.secT, (float)anim.oldT), 1);
                draw.setITSH(UnityEngine.Random.Range((float)anim.secS / 100.0f, (float)anim.oldS / 100.0f), 2);
                draw.setITSH(UnityEngine.Random.Range((float)anim.secH / 360.0f, (float)anim.oldH / 360.0f), 3);
            }
            else if (specialPlaybackMode == specialMode.police)
            {
                //if first half
                if (i < (controlledPixels.Count / 2))
                {
                    if (currentTime < (timelineLength / 2))
                        draw.setITSH((float)anim.oldI / 100.0f, 0);
                    else
                        draw.setITSH(0.0f, 0);
                    draw.setITSH((float)anim.oldT, 1);
                    draw.setITSH((float)anim.oldS / 100.0f, 2);
                    draw.setITSH((float)anim.oldH / 360.0f, 3);
                }
                //if second half
                else
                {
                    if (currentTime > (timelineLength / 2))
                        draw.setITSH((float)anim.secI / 100.0f, 0);
                    else
                        draw.setITSH(0.0f, 0);
                    draw.setITSH((float)anim.oldT, 1);
                    draw.setITSH((float)anim.secS / 100.0f, 2);
                    draw.setITSH((float)anim.secH / 360.0f, 3);
                }
                if (flicker > 0.5)
                {
                    draw.setITSH(0.0f, 0);
                }

            }
            else if (specialPlaybackMode == specialMode.gradient)
            {
                float step = 1.0f / controlledPixels.Count;
                float precent = i * step;
                float invPrecent = 1.0f - precent;
                draw.setITSH((((float)anim.oldI * precent) + ((float)anim.secI * invPrecent)) / 100.0f, 0);
                draw.setITSH((((float)anim.oldT * precent) + ((float)anim.secT * invPrecent)), 1);
                draw.setITSH((((float)anim.oldS * precent) + ((float)anim.secS * invPrecent)) / 100.0f, 2);
                draw.setITSH((((float)anim.oldH * precent) + ((float)anim.secH * invPrecent)) / 360.0f, 3);
            }
            else if (specialPlaybackMode == specialMode.chaser)
            {
                int chaserIndex = (int)(currentTime / (25.0f*3200.0f/(float)anim.secT) + controlledPixels.Count + 1);
                if (controlledPixels.Count != 0)
                {
                    chaserIndex = chaserIndex % (controlledPixels.Count + 1);
                    timelineLength = (25.0f * 3200.0f/ (float)anim.secT) * (float)controlledPixels.Count;
                }

                if (i >= chaserIndex - Mathf.Lerp(1, 85, anim.secS / 100.0f) && i < chaserIndex)
                {
                    draw.setITSH((((float)anim.oldI)) / 100.0f, 0);
                    draw.setITSH((((float)anim.oldT)), 1);
                    draw.setITSH((((float)anim.oldS)) / 100.0f, 2);
                    draw.setITSH((((float)anim.oldH)) / 360.0f, 3);
                }
                else
                {
                    draw.setITSH((((float)anim.secI)) / 100.0f, 0);
                    draw.setITSH((((float)anim.oldT)), 1);
                    draw.setITSH((((float)anim.oldS)) / 100.0f, 2);
                    draw.setITSH((((float)anim.oldH)) / 360.0f, 3);
                }
            }
            else if (specialPlaybackMode == specialMode.mic)
            {
                //TODO: Control brightness by injecting spectrum
                //Let's say we control it with audioIntensity

                //print("mic");
                //Let's start with the easy option! Taking as many spectrum items as we need!

                //Brightness visualization
                float audioIntensityPixel = audioIntensity[i % audioIntensity.Length] / 0.002f;
                //draw.setITSH((float)draw.anim.main.iVal * audioIntensityPixel / 100.0f, 0);
                draw.setITSH(audioIntensityPixel * 0.5f, 0);
                draw.setITSH(0.0f, 1);
                //draw.setITSH((((float)draw.anim.main.sVal) * randomizer[i][0] + ((float)draw.anim.secS) * (1 - randomizer[i][0])) / 100.0f, 2);
                var preIncrement = (BPMincrement + randomizer[i][1]) % 2.0f;
                var increment = -Mathf.Abs(preIncrement - 1.0f) + 1.0f;
                //draw.setITSH((((float)draw.anim.main.hVal) * increment + ((float)draw.anim.secH) * (1 - increment)) / 360.0f, 3);

                ////This is the bar vizualization.
                //float aIntensity = audioIntensity;
                //int BarLEDCount = Convert.ToInt32(Convert.ToSingle(controlledPixels.Count) * aIntensity);
                //if (i <= BarLEDCount) //Turn these on!
                //{

                //}
                //else //Others off!
                //{
                //    draw.setITSH(0.0f, 0);
                //    draw.setITSH(0.0f, 1);
                //    draw.setITSH((float)draw.anim.main.sVal / 100.0f, 2);
                //    draw.setITSH((float)draw.anim.main.hVal / 360.0f, 3);
                //}

            }

            //draw pixel
            //draw.updatePixel(controlledPixels[i]);

        }
	
	}
}
