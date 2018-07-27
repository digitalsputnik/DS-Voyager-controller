using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NatCamU.Core;
using OpenCVForUnity;
using System.Linq;
using Voyager.Lamps;
using Voyager.Workspace;

public class LampDetectionCam : MonoBehaviour
{
    [Space(3)]
	[SerializeField] RawImage preview;
	LampManager lampManager;

    int camIndex;

    byte[] pixelBuffer;
    bool updateCam;
    int camFrameCounter;

    Mat matrix;
    Mat detectionMatrix;

	public List<List<byte[]>> ColorPermutations { get; set; }
	Dictionary<string, byte[]> SsidToPattern = new Dictionary<string, byte[]>();

	void Start()
	{   


		NatCam.Camera = DeviceCamera.Cameras[0];
		NatCam.OnStart += NatCam_OnStart;
		NatCam.OnFrame += NatCam_OnFrame;
		NatCam.Play();

		preview.texture = NatCam.Preview;
	}

	void Update()
	{
		if (updateCam && NatCam.IsPlaying)
			CamUpdate();
	}

	void NatCam_OnStart()
	{
		pixelBuffer = new byte[NatCam.Preview.width * NatCam.Preview.height * 4];
		NatCam.CaptureFrame(pixelBuffer, true);

		InitializeMatrix();
		SetCameraExposuer();
	}

	void NatCam_OnFrame()
	{
		// To let main Update know, that there is a new frame.
		updateCam = true;
	}

	void InitializeMatrix()
	{
		// If matrix is outdated, clean it up and make new one.
		if (matrix != null && (matrix.cols() != NatCam.Preview.width || matrix.rows() != NatCam.Preview.height))
		{
			matrix.Dispose();
			matrix = null;
		}
		matrix = matrix ?? new Mat(NatCam.Preview.height, NatCam.Preview.width, CvType.CV_8UC4);
		Utils.copyToMat(pixelBuffer, matrix);
	}

	public void ChangeCameras()
	{
		camIndex++;
		if (camIndex >= DeviceCamera.Cameras.Length)
			camIndex = 0;

		NatCam.Camera = DeviceCamera.Cameras[camIndex];
	}

	// Called every time new frame is available.
	void CamUpdate()
	{
		HandleCameraMatrix();
		StartCoroutine(ProcessDetectionMatrix());

		camFrameCounter++;
		updateCam = false;
	}

	void SetCameraExposuer()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
#if UNITY_IOS
			if (UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPhone8 || UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPhone8Plus || UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPhoneX)
				NatCam.Camera.ExposureBias = Mathf.Lerp(NatCam.Camera.MinExposureBias, NatCam.Camera.MaxExposureBias, 0.4f);
			else
				NatCam.Camera.ExposureBias = Mathf.Max(NatCam.Camera.MinExposureBias, -20);
#endif
		}
		else
			NatCam.Camera.ExposureBias = Mathf.Max(NatCam.Camera.MinExposureBias, -20);
	}

	void HandleCameraMatrix()
	{
		if (matrix.cols() != NatCam.Preview.width || matrix.rows() != NatCam.Preview.height)
			return;

		NatCam.CaptureFrame(pixelBuffer, true);
		Utils.copyToMat(pixelBuffer, matrix);
		detectionMatrix = matrix.clone();
	}

	IEnumerator ProcessDetectionMatrix()
	{
		// Do the lamp finding here.
		DetectLamps();
		yield return null;
		// Clean up processed matrix. Otherwise device memory will be full in seconds.
		if (detectionMatrix != null) detectionMatrix.Dispose();
		detectionMatrix = null;
	}

	void DetectLamps()
	{
		List<DetectionLampData> detectionLamps = LampImageToLampData();

		foreach (DetectionLampData data in detectionLamps)
		{
			Lamp lamp = lampManager.GetLamp(data.Serial);
			if (lamp == null) continue;

			if (lamp.physicalLamp == null)
			{
				Workspace.InstantiateLamp(lamp, data.lampPoint1, data.lampPoint2);
				Workspace.HideGraphics();
			}
			else
			{
				lamp.physicalLamp.GetComponent<LampMove>().SetPosition(data.lampPoint1, data.lampPoint2);
			}
		}
	}

    void SetupPatterns()
	{
		// TODO: Generate color permutations and assign them to ssids,
        //       use SsidToPattern dictionary that.
	}

	List<DetectionLampData> LampImageToLampData()
	{
		List<DetectionLampData> detectionLamps = new List<DetectionLampData>();

		// Do some magic here. Use detectionMatrix for processing.

		if(lampManager.GetLamps().Count != 0)
		{
			// This is here for testing
            DetectionLampData data = new DetectionLampData()
            {
                Serial = lampManager.GetLamps()[0].Serial,
                lampPoint1 = Vector3.up * 6,
                lampPoint2 = Vector3.right * 10
            };
            detectionLamps.Add(data);
		}

		return detectionLamps;
	}
}

public struct DetectionLampData
{
	public string Serial;
	public Vector3 lampPoint1;
	public Vector3 lampPoint2;
}