using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationCurve : MonoBehaviour {

	public float currentTime;
	public float currentVal;
	public playbackMode mode;

	[System.Serializable]
	public class timePoint {
		public float posInMs;
		public float val;
	}

	public timePoint[] keyframes;

	// Use this for initialization
	void Start () {
		//reorder keys to cronologic order
	}

	public float curveUpdate (float timeIn) {
		//no keyframes
		if (keyframes.Length == 0)
			return 0f;

		//set current time
		currentTime = timeIn;
		float animLength = keyframes [keyframes.Length - 1].posInMs - keyframes [0].posInMs;

		//find closest keyframes
		//if there is more than one keyframe
		if (keyframes.Length > 1) {
			//time is before 1st keyframe
			if (keyframes [0].posInMs >= currentTime) {
				//if hold mode
				if(mode==playbackMode.hold)
					return keyframes [0].val;
				//if loop mode
				if(mode==playbackMode.loop) {
					//move current time within animation spec
					float distance = keyframes[0].posInMs-currentTime;
					float compensation = Mathf.Floor(distance / animLength)+1;
					currentTime = currentTime + (compensation * animLength);
				}
			}
			//time is after last keyframe
			if (keyframes [keyframes.Length-1].posInMs <= currentTime) {
				//if hold mode
				if(mode==playbackMode.hold) 
					return keyframes [keyframes.Length-1].val;
				//if loop mode
				if(mode==playbackMode.loop) {
					float distance = currentTime - keyframes [keyframes.Length - 1].posInMs;
					float compensation = Mathf.Floor(distance / animLength)+1;
					currentTime = currentTime - (compensation * animLength);
				}
			}
			//time is somwhere inbetween
			for (int i=0;i<keyframes.Length;i++) {
				if (keyframes[i].posInMs > currentTime) {
					//distance to previous keyframe
					float dPrevious = currentTime - keyframes[i-1].posInMs;
					//distance between keyframes
					float dKeyframes = keyframes[i].posInMs - keyframes[i-1].posInMs;
					//1st keyframe weight
					float weight = dPrevious / dKeyframes;
					return (keyframes [i - 1].val * (1-weight)) + (keyframes [i].val *  weight);
				}
			}
		} else {
			//single keyframe val
			return keyframes [0].val;
		}

		return 0f;
	}


}


