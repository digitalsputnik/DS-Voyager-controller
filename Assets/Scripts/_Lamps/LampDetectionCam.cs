using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NatCamU.Core;
using OpenCVForUnity;

public class LampDetectionCam : MonoBehaviour {

	[SerializeField] RawImage preview;

    int camIndex;

    byte[] pixelBuffer;
    bool updateCam;
	int camFrameCounter;

	Mat matrix;
	Mat detectionMatrix;

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
		// TODO: This here is for detecting lamps and spawning to the scene, if needed.
        //       detectionMatrix variable and lampManager can be used for that.
	}

    public void ChangeCameras()
	{
		camIndex++;
		if (camIndex >= DeviceCamera.Cameras.Length)
			camIndex = 0;

		NatCam.Camera = DeviceCamera.Cameras[camIndex];
	}
}