/* 
*   NatCam Pro
*   Copyright (c) 2016 Yusuf Olokoba
*/

// Make sure to uncomment '#define OPENCV_API' in NatCam (Assets>NatCam>Pro>Plugins>Managed>NatCam.cs) and in OpenCVBehaviour
#define OPENCV_API // Uncomment this to run this example properly

namespace NatCamU.Examples {

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using Core;
    using Pro;
    #if OPENCV_API
    using OpenCVForUnity;
#endif

    using UnityEngine.UI;

    public class LampContour
    {
        public MatOfPoint contour { get; set; }
        public List<Point> colorCenters { get; set; }
        public Point FirstPoint { get; set; }
        public Point SecondPoint { get; set; }

    }

    public class VisionCam : OpenCVBehaviour {
        
        #if OPENCV_API

        //TODO: getter and setter
		public List<MatOfPoint> finalContours;
		public List<List<MatOfPoint>> ColorContourCollection;

        public List<double[]> lineProperties;

        public Text DebugText;

        /*
         * Slider for exposure control
        public Slider ExposureSlider;


        private void Awake()
        {
            //TODO: Show slider only on iPhone X
            ExposureSlider.onValueChanged.AddListener(OnExposureSliderValueChange);
        }

        private void OnExposureSliderValueChange(float arg0)
        {
            NatCam.Camera.ExposureBias = Mathf.Lerp(NatCam.Camera.MinExposureBias, NatCam.Camera.MaxExposureBias, ExposureSlider.value);  
        }

        */

        public override void OnMatrix () {

            //Flip image!
            Core.flip(matrix, matrix, 0);
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                Imgproc.cvtColor(matrix, matrix, Imgproc.COLOR_BGRA2RGBA);
            }
            
            //Imgproc.drawContours(matrix, finalContours, -1, new Scalar(255, 0, 0));

            if (lineProperties != null)
            {
                for (int i = 0; i < lineProperties.Count(); i++)
                {
                    //TODO: Draw all lines!
                    //lineProperties.Add(new double[] { xs, ys, xe, ye });
                    Imgproc.line(matrix, new Point(lineProperties[i][0], lineProperties[i][1]), new Point(lineProperties[i][2], lineProperties[i][3]), new Scalar(255, 255, 255,255), 3);
                    //Imgproc.circle(matrix, new Point(lineProperties[i][0], lineProperties[i][1]), 10, new Scalar(255, 255, 0, 255), 2);
                }
            }

            //TODO: Draw lines according to lines found!
            //foreach (var contour in finalContours)
            //foreach (var contour in Contours)
            //{
            //    Mat VoyFit = new Mat();
            //    Imgproc.fitLine(contour, VoyFit, Imgproc.DIST_L2, 0, 0.01, 0.01);
            //    Debug.Log("We have fit!");
            //}

            //For debugging

            //if (ColorContourCollection != null && finalContours != null)
            //{
            //    Imgproc.drawContours(matrix, ColorContourCollection[0], -1, new Scalar(255, 0, 0, 255), 2);
            //    Imgproc.drawContours(matrix, ColorContourCollection[1], -1, new Scalar(255, 255, 0, 255), 2);
            //    Imgproc.drawContours(matrix, ColorContourCollection[2], -1, new Scalar(0, 255, 0, 255), 2);
            //    Imgproc.drawContours(matrix, ColorContourCollection[3], -1, new Scalar(0, 0, 255, 255), 2);

            //    Imgproc.drawContours(matrix, finalContours, -1, new Scalar(255, 255, 255, 255), 2);
            //}

            //OnDetectButtonClick();

            // Flush operations on the matrix
            FlushMatrix();
            // Display the result
            //preview.texture = texture;
            preview.Apply(texture);
        }
#endif

        public void OnDetectButtonClick()
        {
            //DontDestroyOnLoad
            Mat rgbMatrix = new Mat();
            Mat hsvMatrix = new Mat();
            Imgproc.cvtColor(matrix, rgbMatrix, Imgproc.COLOR_RGBA2RGB);
            Imgproc.cvtColor(rgbMatrix, hsvMatrix, Imgproc.COLOR_RGB2HSV);

            //Get bright objects
            Mat ThresholdMatrix = new Mat();
            Core.inRange(hsvMatrix, new Scalar(0, 0, 150), new Scalar(180, 255, 255), ThresholdMatrix);

            ThresholdMatrix = morphOps(ThresholdMatrix);

            List<MatOfPoint> Contours = new List<MatOfPoint>();
            Mat Hierarchy = new Mat();
            Imgproc.findContours(ThresholdMatrix, Contours, Hierarchy, Imgproc.RETR_CCOMP, Imgproc.CHAIN_APPROX_SIMPLE);
            

            finalContours = new List<MatOfPoint>();
            finalContours = Contours;

            //Color definitions
            //TODO: Move to some initialization function to avoid regenerating!
            List<Mat> ColorThresholds = new List<Mat>();
            List<Scalar> LowerColors = new List<Scalar>();
            List<Scalar> UpperColors = new List<Scalar>();

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
            Dictionary<MatOfPoint, List<Vector2>> ContourColorPointDictionary = new Dictionary<MatOfPoint, List<Vector2>>();

            foreach (var colorContours in ColorContourCollection)
            {
                List<MatOfPoint> returnContours = new List<MatOfPoint>();
                foreach (var generalContour in finalContours)
                {
                    double previousArea = 0; //Used to get biggest color contour
                    foreach (var colorContour in colorContours)
                    {
                        //Get color Contour centre
                        Moments moments = Imgproc.moments(colorContour);
                        double area = moments.get_m00();
                        if (area <= 0)
                            break;

                        Point colorContourPoint = new Point(moments.get_m10() / area, moments.get_m01() / area);
                        MatOfPoint2f convertedContour = new MatOfPoint2f(generalContour.toArray());
                        if (Imgproc.pointPolygonTest(convertedContour, colorContourPoint, false) > 0)
                        {
                            if (!returnContours.Contains(generalContour))
                            {
                                returnContours.Add(generalContour);
                            }
                            
                            //Add this contourpoint to collection
                            if (ContourColorPointDictionary.Keys.Contains(generalContour))
                            {
                                if (previousArea < area && previousArea > 0)
                                {
                                    ContourColorPointDictionary[generalContour][ContourColorPointDictionary[generalContour].Count - 1] = new Vector2((float)(moments.get_m10() / area), (float)(moments.get_m01() / area));
                                }
                                else if(previousArea == 0)
                                {
                                    ContourColorPointDictionary[generalContour].Add(new Vector2((float)(moments.get_m10() / area), (float)(moments.get_m01() / area)));
                                }
                            }
                            else
                            {
                                ContourColorPointDictionary.Add(generalContour, new List<Vector2> { new Vector2((float)(moments.get_m10() / area), (float)(moments.get_m01() / area)) });
                            }
                            previousArea = area;
                        }
                    }
                }
                finalContours = returnContours;
            }

            lineProperties = new List<double[]>();

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
                    lineProperties.Add(new double[] { xs, ys, xe, ye }); //TODO: Remove
                }
                else
                {
                    xs = x0 - width / 2;
                    xe = x0 + width / 2;
                    ys = (vy / vx) * (xs - x0) + y0;
                    ye = (vy / vx) * (xe - x0) + y0;
                    lineProperties.Add(new double[] { xs, ys, xe, ye }); //TODO: Remove
                }

                ContourToEdgePointsDictionary.Add(contour, new List<Vector2> { new Vector2((float)xs, (float)ys), new Vector2((float)xe, (float)ye) });
            }

            //Generate same permutations
            int[][] permutations = new int[][]
            {
                new int[] {1,2,3,4 },
                new int[] {1,2,4,3 },
                new int[] {1,3,2,4 },
                new int[] {1,3,4,2 },
                new int[] {1,4,2,3 },
                new int[] {1,4,3,2 },
                new int[] {2,1,3,4 },
                new int[] {2,1,4,3 },
                new int[] {2,3,1,4 },
                new int[] {2,4,1,3 },
                new int[] {3,1,2,4 },
                new int[] {3,2,1,4 }
            };

            //Get available lamps
            var AvailableLamps = this.GetComponent<LampCommunication>().lampColors;

            int permutationIndex = 0;

            //Add IP and send with some DTO
            var DetectedLampsGameObject = GameObject.Find("DetectedLampProperties");
            var DetectedLampsDTO = DetectedLampsGameObject.GetComponent<DetectedLampProperties>().DetectedLamps;

            DetectedLampsDTO.Clear();

            if (finalContours.Count == 0 || AvailableLamps.Count == 0)
            {
                return;
            }

            Dictionary<float, string> functionValueToIPDictionary = new Dictionary<float, string>();

            //Objective Function = f(IP, Contour)
            float[][] objFunction = new float[AvailableLamps.Count][];

            Dictionary<float, List<Vector2>> functionValueToPointsDictionary = new Dictionary<float, List<Vector2>>();

            for (int l = 0; l < AvailableLamps.Count; l++)
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
                    for (int k = 0; k < LowerColors.Count; k++)
                    {
                        var Pc = ContourColorPointDictionary[contour][permutations[l][k] - 1];
                        startFunction += (((2f * k + 1f) / 8f) - (Pc - start).magnitude / contourLength) * (((2f * k + 1f) / 8f) - (Pc - start).magnitude / contourLength);
                        endFunction += (((2 * k + 1) / 8)  - (Pc - end).magnitude / contourLength) * (((2 * k + 1) / 8) - (Pc - end).magnitude / contourLength);

                        previousStartVector = previousStartVector < (Pc - start).magnitude ? (Pc - start).magnitude : float.MaxValue;
                        previousEndVector = previousEndVector < (Pc - end).magnitude ? (Pc - end).magnitude : float.MaxValue;
                        startVectors.Add((Pc - start).magnitude);
                        endVectors.Add((Pc - end).magnitude);
                    }

                    //DebugText.text = "start + " + string.Join(",", startVectors.Select(x => x.ToString()).ToArray())
                    //    + "\n end " + string.Join(",", endVectors.Select(x => x.ToString()).ToArray());

                    startFunction = previousStartVector == float.MaxValue ? startFunction*100f : startFunction;
                    endFunction = previousEndVector == float.MaxValue ? endFunction*100f : endFunction;

                    if (!functionValueToPointsDictionary.ContainsKey(startFunction))
                        functionValueToPointsDictionary.Add(startFunction, new List<Vector2> { end, start });

                    if (!functionValueToPointsDictionary.ContainsKey(endFunction))
                        functionValueToPointsDictionary.Add(endFunction, new List<Vector2> { start, end });

                    objFunValuesForContour.Add(Mathf.Min(startFunction,endFunction));
                }

                objFunction[l] = objFunValuesForContour.ToArray();
            }

            //Get minimum values until no more contours or IPs
            List<int> excludedContourIndex = new List<int>();
            List<int> excludedIPIndex = new List<int>();
            var min = objFunction.SelectMany((subArr, i) => subArr.Select((value, j) => new { i, j, value }))
                .OrderBy(x => x.value)
                .ToArray();

            for (int k = 0; k < min.Length; k++)
            {
                if (excludedIPIndex.Count == AvailableLamps.Count || excludedContourIndex.Count == finalContours.Count)
                    break;

                if (!excludedIPIndex.Contains(min[k].i) && !excludedContourIndex.Contains(min[k].j))
                {
                    //Get start and end points
                    var points = functionValueToPointsDictionary[min[k].value];

                    DetectedLampsDTO.Add(new LampProperties
                    {
                        IP = AvailableLamps[min[k].i].IP,
                        EndPoint = TransFormPoint(points[0]),
                        StartPoint = TransFormPoint(points[1]),
                        LampLength = AvailableLamps[min[k].i].LampLength,
                        batteryLevel = AvailableLamps[min[k].i].BatteryLevel,
                        macName = AvailableLamps[min[k].i].MacName
                    });

                    excludedIPIndex.Add(min[k].i);
                    excludedContourIndex.Add(min[k].j);
                }
            }

            for (int i = 0; i < AvailableLamps.Count; i++)
            {
                if (!excludedIPIndex.Contains(i))
                {
                    DetectedLampsGameObject.GetComponent<DetectedLampProperties>().LampIPtoLengthDictionary
                        .Remove(IPAddress.Parse(AvailableLamps[i].IP));
                }
            }
        }

        private static Vector2 TransFormPoint(Vector2 point)
        {
            var ScreenDimensions = NatCam.Camera.PreviewResolution;
            var transFormedPoint = point;
            transFormedPoint.x = Mathf.Max(Mathf.Min(transFormedPoint.x, ScreenDimensions.x), 0f);
            transFormedPoint.y = Mathf.Max(Mathf.Min(transFormedPoint.y, ScreenDimensions.y), 0f);
            transFormedPoint.x = transFormedPoint.x / ScreenDimensions.x;
            transFormedPoint.y = (ScreenDimensions.y - transFormedPoint.y) / ScreenDimensions.y;
            return transFormedPoint;
        }

        public void OnBackButtonClick()
        {
            var DetectedLampsGameObject = GameObject.Find("DetectedLampProperties");
            DetectedLampsGameObject.GetComponent<DetectedLampProperties>().AddLamps = true;
            SceneManager.LoadScene("Main");
            SceneManager.UnloadSceneAsync("VisionCam");
        }

        /// <summary>
        /// Morphs the ops.
        /// </summary>
        /// <param name="thresh">Thresh.</param>
        private Mat morphOps (Mat thresh)
		{
			//create structuring element that will be used to "dilate" and "erode" image.
			//the element chosen here is a 3px by 3px rectangle
			//Mat erodeElement = Imgproc.getStructuringElement (Imgproc.MORPH_RECT, new Size (3, 3));
			//dilate with larger element so make sure object is nicely visible
			Mat dilateElement = Imgproc.getStructuringElement (Imgproc.MORPH_RECT, new Size (10, 10));

			//Imgproc.erode (thresh, thresh, erodeElement);

            Imgproc.dilate(thresh, thresh, dilateElement);

            return thresh;
		}
    }
}