using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NatCamU.Core;
using OpenCVForUnity;
using System.Linq;
using Voyager.Lamps;
using Voyager.Workspace;
using Voyager.Networking;

public class LampDetectionCam : MonoBehaviour
{
    [Space(3)]
	[SerializeField] RawImage preview;
	[SerializeField] LampManager lampManager;
	[SerializeField] NetworkManager networkManager;
	[SerializeField] float SetDetectionInterval = 0.5f;

    int camIndex;

    byte[] pixelBuffer;
    bool updateCam;
    int camFrameCounter;

    Mat matrix;
    Mat detectionMatrix;

	public List<List<byte[]>> ColorPermutations { get; set; }
	Dictionary<string, int[]> SsidToPattern = new Dictionary<string, int[]>();
	public List<byte[]> CalibrationColors { get; set; }

	//Color limits for detecting colors from image
	List<Scalar> LowerColors = new List<Scalar>();
	List<Scalar> UpperColors = new List<Scalar>();

	//Contours for detection
	//Detection
	public List<MatOfPoint> finalContours;
	public List<List<MatOfPoint>> ColorContourCollection;
	public Dictionary<MatOfPoint, List<Vector2>> ContourColorPointDictionary;

	Vector3 WorldPointCorner1;
	Vector3 WorldPointCorner2;
	public float worldPointWidth;
	float worldPointHeight;

	void Start()
	{   
		InitializeColorLimits ();

		//Camera initialization
		NatCam.Camera = DeviceCamera.Cameras[0];
		NatCam.OnStart += NatCam_OnStart;
		NatCam.OnFrame += NatCam_OnFrame;
		NatCam.Play();

		preview.texture = NatCam.Preview;
		InvokeRepeating("SetDetectionModes", 2.0f, SetDetectionInterval);

		SetupWorldPoints();
	}

	void Update()
	{
		SetupPatterns ();
		StartCoroutine (SendPatternToLamps ());

		if (updateCam && NatCam.IsPlaying)
			CamUpdate();
	}

    void SetupWorldPoints()
	{
		Ray ray;
        Plane hPlane = new Plane(Vector3.back, Vector3.zero);
        float distance = 0;

        ray = Camera.main.ScreenPointToRay(new Vector3(0.0f, 0.0f, 0.0f));
        if (hPlane.Raycast(ray, out distance))
            WorldPointCorner1 = ray.GetPoint(distance);

        ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width, Screen.height, 0.0f));
        if (hPlane.Raycast(ray, out distance))
            WorldPointCorner2 = ray.GetPoint(distance);

		worldPointWidth = WorldPointCorner2.x - WorldPointCorner1.x;
		worldPointHeight = WorldPointCorner2.y - WorldPointCorner1.y;
	}

    void SetDetectionModes()
	{
		foreach (Lamp lamp in lampManager.GetLamps())
			NetworkManager.SetDetectionMode(lamp.IP, true);
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

	IEnumerator SendPatternToLamps()
	{
		foreach (var SerialPatternPair in SsidToPattern) {
			var lamp = lampManager.GetLamp (SerialPatternPair.Key);
			var lightData = GenerateColorPacket(lamp.Lenght, SerialPatternPair.Value);
			networkManager.SendMessage (lamp.IP, lightData);
		}
		yield return null;
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

		List<WorkspaceItem> items = new List<WorkspaceItem>(Workspace.GetItemsInWorkspace());
		foreach(WorkspaceItem item in items)
		{
			if(item.Type == WorkspaceItem.WorkspaceItemType.Lamp)
			{
				PhysicalLamp lamp = item.GetComponent<PhysicalLamp>();
				DetectionLampData detectionLamp = detectionLamps.Find(x => x.Serial == lamp.Owner.Serial);
				if (detectionLamp == null)
					Workspace.DestroyItem(item);
            }
		}
	}

	/// <summary>
	/// Sets up pattern for each applicable (detectable) lamp.
	/// </summary>
	void SetupPatterns()
	{
		//Calibration colors
		CalibrationColors = new List<byte[]>();
		CalibrationColors.Add(new byte[] { 100, 0, 0, 0 });
		CalibrationColors.Add(new byte[] { 100, 100, 0, 0 });
		CalibrationColors.Add(new byte[] { 0, 100, 0, 0 });
		CalibrationColors.Add(new byte[] { 0, 0, 100, 0 });

		//Permutations
		int[][] permutations =
		{
			new int[] {1,2,3,4},
			new int[] {1,2,4,3},
			new int[] {1,3,2,4},
			new int[] {1,3,4,2},
			new int[] {1,4,2,3},
			new int[] {1,4,3,2},
			new int[] {2,1,3,4},
			new int[] {2,1,4,3},
			new int[] {2,3,1,4},
			new int[] {2,4,1,3},
			new int[] {3,1,2,4},
			new int[] {3,2,1,4}
		};

		var lamps = lampManager.GetLamps ();

		//SsidToPattern.Clear ();

		int permutationIndex = 0;
		foreach (var lamp in lamps) {
			if (lamp.Type == LampType.Voyager3_2ft || lamp.Type == LampType.Voyager3_4ft || lamp.Type == LampType.Voyager_2ft || lamp.Type == LampType.Voyager_4ft) {
				if (SsidToPattern.ContainsKey (lamp.Serial)) {
					SsidToPattern[lamp.Serial] = permutations [permutationIndex];
				} else {
					SsidToPattern.Add (lamp.Serial, permutations [permutationIndex]);
				}


				permutationIndex += 1;
			}

			if (permutationIndex >= permutations.Length) {
				break;
			}
		}
	}

	byte[] GenerateColorPacket(int pixelCount, int[] permutation)
	{
		byte[] data = new byte[343];
		byte[] lightValues = new byte[332];

		//header
		//RGBW
		Buffer.BlockCopy(new byte[] { 0xD5, 0x0A, 0x10, 0x03 }, 0, data, 0, 4);
		//General color sending!
		Buffer.BlockCopy(new byte[] { 0x00, 0x00, 0x00, Convert.ToByte(pixelCount) }, 0, data, 4, 4);
		//terminator + empty checksum
		Buffer.BlockCopy(new byte[] { 0xEF, 0xFE, 0x00 }, 0, data, 8 + 4*pixelCount, 3);
        
		int lightStep = pixelCount / CalibrationColors.Count;

		for (int pixNr = 0; pixNr < pixelCount; pixNr++)
		{
			int colorIndex = (pixNr / lightStep) == CalibrationColors.Count ? CalibrationColors.Count - 1 : pixNr / lightStep;
			Buffer.BlockCopy (CalibrationColors [permutation [colorIndex] - 1], 0, lightValues, pixNr * 4, 4);
		}

		Buffer.BlockCopy(lightValues, 0, data, 8, 4 * pixelCount);

		return data;
	}

	List<DetectionLampData> LampImageToLampData()
	{
		List<DetectionLampData> detectionLamps = new List<DetectionLampData>();

		// TODO: Do some magic here. Use detectionMatrix for processing.
		//Matrix manipulations
		Mat rgbMatrix = new Mat();
		Mat hsvMatrix = new Mat();
		Imgproc.cvtColor(detectionMatrix, rgbMatrix, Imgproc.COLOR_RGBA2RGB);
		Imgproc.cvtColor(rgbMatrix, hsvMatrix, Imgproc.COLOR_RGB2HSV);
		//yield return null;

		//Get bright objects
		Mat ThresholdMatrix = new Mat();
		Core.inRange(hsvMatrix, new Scalar(0, 0, 150), new Scalar(180, 255, 255), ThresholdMatrix);
		ThresholdMatrix = morphOps(ThresholdMatrix);

		List<MatOfPoint> Contours = new List<MatOfPoint>();
		Mat Hierarchy = new Mat();
		Imgproc.findContours(ThresholdMatrix, Contours, Hierarchy, Imgproc.RETR_CCOMP, Imgproc.CHAIN_APPROX_SIMPLE);
		//yield return null;

		//Draw contours
		ColorContourCollection = new List<List<MatOfPoint>>();
		for (int i = 0; i < LowerColors.Count; i++)
		{
			//Theshold
			Mat ColorThresholdMatrix = new Mat();
			Core.inRange(hsvMatrix, LowerColors[i], UpperColors[i], ColorThresholdMatrix);

			//Morphing
			ColorThresholdMatrix = morphOps(ColorThresholdMatrix);

			//Contours
			List<MatOfPoint> ColorContours = new List<MatOfPoint>();
			Mat ColorHierarchy = new Mat();
			Imgproc.findContours(ColorThresholdMatrix, ColorContours, ColorHierarchy, Imgproc.RETR_CCOMP, Imgproc.CHAIN_APPROX_SIMPLE);
			ColorContourCollection.Add(ColorContours);
		}

		//Create Contour -> ColorPointDictionary
		ContourColorPointDictionary = new Dictionary<MatOfPoint, List<Vector2>>();
		CreateColorContourCentres (Contours);
		//yield return null;


		Dictionary<MatOfPoint, List<Vector2>> ContourToEdgePointsDictionary = new Dictionary<MatOfPoint, List<Vector2>>();
		foreach (var contour in finalContours)
		{
			Mat VoyFit = new Mat();
			Imgproc.fitLine(contour, VoyFit, Imgproc.DIST_L2, 0, 0.01, 0.01);

			var boundingRect = Imgproc.boundingRect(contour);

			double height = Convert.ToDouble(boundingRect.height);
			double width = Convert.ToDouble(boundingRect.width);
			double vx = VoyFit.get(0, 0)[0];
			double vy = VoyFit.get(1, 0)[0];
			double x0 = VoyFit.get(2, 0)[0];
			double y0 = VoyFit.get(3, 0)[0];

			double xs = 0;
			double xe = 0;
			double ys = 0;
			double ye = 0;

			//Convert to start and end points!
			if (vx < vy)
			{
				ys = y0 - height / 2;
				ye = y0 + height / 2;
				xs = (vx / vy) * (ys - y0) + x0;
				xe = (vx / vy) * (ye - y0) + x0;
			}
			else
			{
				xs = x0 - width / 2;
				xe = x0 + width / 2;
				ys = (vy / vx) * (xs - x0) + y0;
				ye = (vy / vx) * (xe - x0) + y0;
			}

			ContourToEdgePointsDictionary.Add(contour, new List<Vector2> { new Vector2((float)xs, (float)ys), new Vector2((float)xe, (float)ye) });
		}
		//yield return null;

		//Objective Function = f(IP, Contour)
		float[][] objFunction = new float[SsidToPattern.Count()][];

		Dictionary<float, List<Vector2>> functionValueToPointsDictionary = new Dictionary<float, List<Vector2>>();

		for (int l = 0; l < SsidToPattern.Count(); l++)
		{
			List<float> objFunValuesForContour = new List<float>();
			foreach (var contour in finalContours)
			{
				Vector2 start = ContourToEdgePointsDictionary[contour].First();
				Vector2 end = ContourToEdgePointsDictionary[contour].Last();
				float contourLength = (end - start).magnitude;

				//Calculation of function
				float startFunction = 0f;
				float endFunction = 0f;
				float previousStartVector = 0f;
				float previousEndVector = 0f;
				List<float> startVectors = new List<float>();
				List<float> endVectors = new List<float>();
				int[][] perms = SsidToPattern.Values.ToArray ();
				for (int k = 0; k < LowerColors.Count; k++)
				{
					var Pc = ContourColorPointDictionary[contour][perms[l][k] - 1];
					startFunction += (((2f * k + 1f) / 8f) - (Pc - start).magnitude / contourLength) * (((2f * k + 1f) / 8f) - (Pc - start).magnitude / contourLength);
					endFunction += (((2 * k + 1) / 8) - (Pc - end).magnitude / contourLength) * (((2 * k + 1) / 8) - (Pc - end).magnitude / contourLength);

					previousStartVector = previousStartVector < (Pc - start).magnitude ? (Pc - start).magnitude : float.MaxValue;
					previousEndVector = previousEndVector < (Pc - end).magnitude ? (Pc - end).magnitude : float.MaxValue;
					startVectors.Add((Pc - start).magnitude);
					endVectors.Add((Pc - end).magnitude);
				}
                            
				startFunction = previousStartVector.Equals(float.MaxValue) ? startFunction * 100f : startFunction;
				endFunction = previousEndVector.Equals(float.MaxValue) ? endFunction * 100f : endFunction;

				if (!functionValueToPointsDictionary.ContainsKey(startFunction))
					functionValueToPointsDictionary.Add(startFunction, new List<Vector2> { end, start });

				if (!functionValueToPointsDictionary.ContainsKey(endFunction))
					functionValueToPointsDictionary.Add(endFunction, new List<Vector2> { start, end });

				objFunValuesForContour.Add(Mathf.Min(startFunction, endFunction));
			}

			objFunction[l] = objFunValuesForContour.ToArray();
		}

		//yield return null;

		List<int> excludedContourIndex = new List<int>();
		List<int> excludedMacIndex = new List<int>();
		var min = objFunction.SelectMany((subArr, i) => subArr.Select((value, j) => new { i, j, value }))
			.OrderBy(x => x.value)
			.ToArray();

		List<string> LampSerialsForDetection = SsidToPattern.Keys.ToList ();

		for (int k = 0; k < min.Length; k++)
		{
			//Get start and end points
			var points = functionValueToPointsDictionary[min[k].value];

			detectionLamps.Add (new DetectionLampData {
				Serial = LampSerialsForDetection[min[k].i],
				lampPoint1 = TransformPoint(points[0]),
				lampPoint2 = TransformPoint(points[1])
			});         
		}

		return detectionLamps;
	}

	void CreateColorContourCentres (List<MatOfPoint> Contours)
	{
		ContourColorPointDictionary = new Dictionary<MatOfPoint, List<Vector2>> ();
		finalContours = new List<MatOfPoint> ();
		finalContours = Contours;
		foreach (var colorContours in ColorContourCollection) {
			List<MatOfPoint> returnContours = new List<MatOfPoint> ();
			foreach (var generalContour in finalContours) {
				double previousArea = 0;
				//Used to get biggest color contour
				foreach (var colorContour in colorContours) {
					//Get color Contour centre
					Moments moments = Imgproc.moments (colorContour);
					double area = moments.get_m00 ();
					if (area <= 0)
						break;
					Point colorContourPoint = new Point (moments.get_m10 () / area, moments.get_m01 () / area);
					MatOfPoint2f convertedContour = new MatOfPoint2f (generalContour.toArray ());
					if (Imgproc.pointPolygonTest (convertedContour, colorContourPoint, false) > 0) {
						if (!returnContours.Contains (generalContour)) {
							returnContours.Add (generalContour);
						}
						//Add this contourpoint to collection
						if (ContourColorPointDictionary.ContainsKey (generalContour)) {
							if (previousArea < area && previousArea > 0) {
								ContourColorPointDictionary [generalContour] [ContourColorPointDictionary [generalContour].Count - 1] = new Vector2 ((float)(moments.get_m10 () / area), (float)(moments.get_m01 () / area));
							}
							else
								if (previousArea.Equals(0.0f)) {
									ContourColorPointDictionary [generalContour].Add (new Vector2 ((float)(moments.get_m10 () / area), (float)(moments.get_m01 () / area)));
								}
						}
						else {
							ContourColorPointDictionary.Add (generalContour, new List<Vector2> {
								new Vector2 ((float)(moments.get_m10 () / area), (float)(moments.get_m01 () / area))
							});
						}
						previousArea = area;
					}
				}
			}
			finalContours = returnContours;
		}
	}

	/// <summary>
	/// Transforms point according to resolution
	/// </summary>
	/// <returns>Transformed point.</returns>
	/// <param name="point">Initial point.</param>
	Vector3 TransformPoint(Vector2 point)
	{
		CameraResolution imageDimentions = NatCam.Camera.PreviewResolution;
        Vector3 transformedPoint = new Vector3(point.x, point.y, 0.0f);

		transformedPoint.x = Mathf.Max(Mathf.Min(transformedPoint.x, imageDimentions.width), 0f);
		transformedPoint.y = Mathf.Max(Mathf.Min(transformedPoint.y, imageDimentions.height), 0f);
		transformedPoint.x = transformedPoint.x / imageDimentions.width;
		transformedPoint.y = (imageDimentions.height - transformedPoint.y) / imageDimentions.height;

		transformedPoint.x = WorldPointCorner1.x + (transformedPoint.x * worldPointWidth);
		transformedPoint.y = WorldPointCorner1.y + (transformedPoint.y * worldPointHeight);

		return transformedPoint;
	}
       
	#region Image Manipulation routines
	/// <summary>
	/// Performs morphological operations on image to make it discoverable
	/// </summary>
	/// <param name="thresh">Thresh.</param>
	private Mat morphOps(Mat thresh)
	{
		//create structuring element that will be used to "dilate" and "erode" image.
		//the element chosen here is a 10px by 10px rectangle
		//dilate with larger element so make sure object is nicely visible
		Mat dilateElement = Imgproc.getStructuringElement(Imgproc.MORPH_RECT, new Size(10, 10));

		//Imgproc.erode (thresh, thresh, erodeElement);
		Imgproc.dilate(thresh, thresh, dilateElement);

		return thresh;
	}
#endregion

	/// <summary>
	/// Sets color limits for detection for each particular color.
	/// </summary>
	void InitializeColorLimits()
	{
		//NB! This needs to be in the seed permutation order. Currently: RYGB
		//Red
		LowerColors.Add(new Scalar(-20, 50, 200));
		UpperColors.Add(new Scalar(20, 255, 255));

		//yellow
		LowerColors.Add(new Scalar(20, 50, 200));
		UpperColors.Add(new Scalar(40, 255, 255));

		//Green
		LowerColors.Add(new Scalar(50, 50, 200));
		UpperColors.Add(new Scalar(100, 255, 255));

		//Blue
		LowerColors.Add(new Scalar(100, 50, 200));
		UpperColors.Add(new Scalar(130, 255, 255));
	}

}

public class DetectionLampData
{
	public string Serial;
	public Vector3 lampPoint1;
	public Vector3 lampPoint2;
}