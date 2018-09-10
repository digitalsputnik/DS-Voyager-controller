using UnityEngine;
using NatCamU.Core;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Net;
using UnityEngine.UI;
using OpenCVForUnity;

#if UNITY_5_3 || UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif

namespace NatCamWithOpenCVForUnityExample
{
    /// <summary>
    /// NatCamPreview To Mat Example
    /// An example of converting a NatCam preview image to OpenCV's Mat format.
    /// </summary>
    public class NatCamPreviewToMatExample : MonoBehaviour
    {
        public enum MatCaptureMethod
        {
            NatCam_CaptureFrame,
            NatCam_CaptureFrame_OpenCVFlip,
            BlitWithReadPixels,
            Graphics_CopyTexture,
        }

        public enum ImageProcessingType
        {
            None,
            DrawLine,
            ConvertToGray,
        }

        // An image flipping method for returning a display image from the OpenCV coordinate system (Y - 0 is the top of the image) to the OpenGL coordinate system (Y - 0 is the bottom of the image).
        public enum ImageFlippingMethod
        {
            OpenCVForUnity_Flip,
            Shader,
        }

        [Header("Camera")]
        public bool useFrontCamera;

        [Header("Preview")]
        public RawImage preview;
        public CameraResolution previewResolution = CameraResolution._1280x720;
        public int requestedFPS = 30;
        public AspectRatioFitter aspectFitter;
        public ImageFlippingMethod imageFlippingMethod = ImageFlippingMethod.OpenCVForUnity_Flip;
        public Dropdown imageFlippingMethodDropdown; 
        Material originalMaterial;
        Material viewMaterial;

        [Header("OpenCV")]
        public MatCaptureMethod matCaptureMethod = MatCaptureMethod.NatCam_CaptureFrame;
        public Dropdown matCaptureMethodDropdown; 
        public ImageProcessingType imageProcessingType = ImageProcessingType.None;
        public Dropdown imageProcessingTypeDropdown; 

        bool didUpdateThisFrame = false;

        int updateCount = 0;
        int onFrameCount = 0;
        int drawCount = 0;

        float elapsed = 0;
        float updateFPS = 0;
        float onFrameFPS = 0;
        float drawFPS = 0;

        int CameraIndex = 0;
        DeviceCamera[] cameras;

        /// <summary>
        /// The FPS monitor.
        /// </summary>
        FpsMonitor fpsMonitor;

        Mat matrix;
        Mat matrixForDetection;
        Mat grayMatrix;
        byte[] pixelBuffer;
        Texture2D texture;
        const TextureFormat textureFormat = TextureFormat.RGBA32;

        //Detection
        public List<MatOfPoint> finalContours;
        public List<List<MatOfPoint>> ColorContourCollection;

        public List<double[]> lineProperties;

        public virtual void Start () 
        {
            fpsMonitor = GetComponent<FpsMonitor> ();

            if (!NatCam.Implementation.HasPermissions) {
                Debug.LogError ("NatCam.Implementation.HasPermissions == false");

                if (fpsMonitor != null)
                    fpsMonitor.consoleText = "NatCam.Implementation.HasPermissions == false";

                return;
            }

            // Load global camera benchmark settings.
            int width, height, fps; 
            NatCamWithOpenCVForUnityExample.GetCameraResolution (out width, out height);
            NatCamWithOpenCVForUnityExample.GetCameraFps (out fps);
            previewResolution = new NatCamU.Core.CameraResolution(width, height);
            requestedFPS = fps;

            cameras = DeviceCamera.Cameras;

            // Set the active camera
            NatCam.Camera = cameras[CameraIndex];//useFrontCamera ? DeviceCamera.FrontCamera : DeviceCamera.RearCamera;

            // Null checking
            if (!NatCam.Camera) {
                Debug.LogError("Camera is null. Consider using "+(useFrontCamera ? "rear" : "front")+" camera");
                return;
            }
            if (!preview) {
                Debug.LogError("Preview RawImage has not been set");
                return;
            }
                
            SetMaterials ();

            // Set the camera's preview resolution
            NatCam.Camera.PreviewResolution = previewResolution;
            //NatCam.Camera.ExposureBias = -20f;
            // Set the camera framerate
            NatCam.Camera.Framerate = requestedFPS;
            NatCam.Play();
            NatCam.OnStart += OnStart;
            NatCam.OnFrame += OnFrame;

            if (fpsMonitor != null){
                fpsMonitor.Add ("Name", "NatCamPreviewToMatExample");
                fpsMonitor.Add ("onFrameFPS", onFrameFPS.ToString("F1"));
                fpsMonitor.Add ("drawFPS", drawFPS.ToString("F1"));
                fpsMonitor.Add ("width", "");
                fpsMonitor.Add ("height", "");
                fpsMonitor.Add ("orientation", "");
            }
                
            matCaptureMethodDropdown.value = (int)matCaptureMethod;
            imageProcessingTypeDropdown.value = (int)imageProcessingType;
            imageFlippingMethodDropdown.value = (int)imageFlippingMethod;
        }

        /// <summary>
        /// Method called when the camera preview starts
        /// </summary>
        public virtual void OnStart ()
        {
            // Create pixel buffer
            pixelBuffer = new byte[NatCam.Preview.width * NatCam.Preview.height * 4];

            // Get the preview data
            NatCam.CaptureFrame(pixelBuffer, true);

            // Create preview matrix
            if (matrix != null && (matrix.cols() != NatCam.Preview.width || matrix.rows() != NatCam.Preview.height)) {
                matrix.Dispose ();
                matrix = null;
            }
            matrix = matrix ?? new Mat(NatCam.Preview.height, NatCam.Preview.width, CvType.CV_8UC4);
            Utils.copyToMat (pixelBuffer, matrix);

            // Create display texture
            if (texture && (texture.width != matrix.cols() || texture.height != matrix.rows())) {
                Texture2D.Destroy (texture);
                texture = null;
            }
            texture = texture ?? new Texture2D (matrix.cols(), matrix.rows(), textureFormat, false, false);

            // Scale the panel to match aspect ratios
            aspectFitter.aspectRatio = NatCam.Preview.width / (float)NatCam.Preview.height;

            // Display the result
            preview.texture = texture;

            Debug.Log ("OnStart (): " + matrix.cols() + " " + matrix.rows() + " " + NatCam.Preview.width + " " + NatCam.Preview.height + " " + texture.width + " " + texture.height);

			if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
            #if UNITY_IOS
                if (UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPhone8 ||
                    UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPhone8Plus ||
                    UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPhoneX)
                {
                    NatCam.Camera.ExposureBias = Mathf.Lerp(NatCam.Camera.MinExposureBias, NatCam.Camera.MaxExposureBias, 0.4f);
                }
                else
                    NatCam.Camera.ExposureBias = Mathf.Max(NatCam.Camera.MinExposureBias, -20);
            #endif
            }
            else
				try { NatCam.Camera.ExposureBias = Mathf.Max(NatCam.Camera.MinExposureBias, -20); }
				catch (Exception) { }

            StartCoroutine("DetectLampsCoroutine");
        }

        /// <summary>
        /// Method called on every frame that the camera preview updates
        /// </summary>
        public virtual void OnFrame ()
        {
            onFrameCount++;

            didUpdateThisFrame = true;
        }

        // Update is called once per frame
        void Update()
        {
            updateCount++;
            elapsed += Time.deltaTime;
            if (elapsed >= 1f)
            {
                updateFPS = updateCount / elapsed;
                onFrameFPS = onFrameCount / elapsed;
                drawFPS = drawCount / elapsed;
                updateCount = 0;
                onFrameCount = 0;
                drawCount = 0;
                elapsed = 0;

                Debug.Log("didUpdateThisFrame: " + didUpdateThisFrame + " updateFPS: " + updateFPS + " onFrameFPS: " + onFrameFPS + " drawFPS: " + drawFPS);
                if (fpsMonitor != null)
                {
                    fpsMonitor.Add("onFrameFPS", onFrameFPS.ToString("F1"));
                    fpsMonitor.Add("drawFPS", drawFPS.ToString("F1"));

                    if (matrix != null)
                    {
                        fpsMonitor.Add("width", matrix.width().ToString());
                        fpsMonitor.Add("height", matrix.height().ToString());
                    }
                    fpsMonitor.Add("orientation", Screen.orientation.ToString());
                }
            }

            if (NatCam.IsPlaying && didUpdateThisFrame) {    

                drawCount++;

                Mat matrix = GetMat (matCaptureMethod);

                if (matrix != null) {

                    //ProcessImage (matrix, imageProcessingType);

                    // The Imgproc.putText method is too heavy to use for mobile device benchmark purposes.
                    //Imgproc.putText (matrix, "W:" + matrix.width () + " H:" + matrix.height () + " SO:" + Screen.orientation, new Point (5, matrix.rows () - 10), Core.FONT_HERSHEY_SIMPLEX, 1.0, new Scalar (255, 255, 255, 255), 2, Imgproc.LINE_AA, false);
                    //Imgproc.putText (matrix, "updateFPS:" + updateFPS.ToString("F1") + " onFrameFPS:" + onFrameFPS.ToString("F1") + " drawFPS:" + drawFPS.ToString("F1"), new Point (5, matrix.rows () - 50), Core.FONT_HERSHEY_SIMPLEX, 1.0, new Scalar (255, 255, 255, 255), 2, Imgproc.LINE_AA, false);

                    matrixForDetection = matrix.clone();

                    if (lineProperties != null)
                    {
                        for (int i = 0; i < lineProperties.Count; i++)
                        {
                            //TODO: Draw all lines!
                            //lineProperties.Add(new double[] { xs, ys, xe, ye });
                            Imgproc.line(matrix, new Point(lineProperties[i][0], lineProperties[i][1]), new Point(lineProperties[i][2], lineProperties[i][3]), new Scalar(255, 255, 255, 255), 3);
                            //Imgproc.circle(matrix, new Point(lineProperties[i][0], lineProperties[i][1]), 10, new Scalar(255, 255, 0, 255), 2);
                        }
                    }

                    Utils.fastMatToTexture2D(matrix, texture, true, 0, true);
                    //switch (imageFlippingMethod) {
                    //default:
                    //case ImageFlippingMethod.OpenCVForUnity_Flip:
                    //    // Restore the coordinate system of the image by OpenCV's Flip function.
                    //    Utils.fastMatToTexture2D (matrix, texture, true, 0, false);
                    //    break;
                    //case ImageFlippingMethod.Shader:
                    //    // Restore the coordinate system of the image by Shader. (use GPU)
                    //    Utils.fastMatToTexture2D (matrix, texture, false, 0, false);
                    //    break;
                    //}
                }
            }
        }
       
        void LateUpdate ()
        {
            didUpdateThisFrame = false;
        }
            
        /// <summary>
        /// Gets the current camera preview frame that converted to the correct direction in OpenCV Matrix format.
        /// </summary>
        private Mat GetMat (MatCaptureMethod matCaptureMethod = MatCaptureMethod.NatCam_CaptureFrame)
        {
            if (matrix.cols () != NatCam.Preview.width || matrix.rows () != NatCam.Preview.height)
                return null;

            switch (matCaptureMethod) {
            default:
            case MatCaptureMethod.NatCam_CaptureFrame:
                // Get the preview data
                // Set `flip` flag to true because OpenCV uses inverted Y-coordinate system
                NatCam.CaptureFrame(pixelBuffer, true);

                Utils.copyToMat (pixelBuffer, matrix);

                break;
            case MatCaptureMethod.NatCam_CaptureFrame_OpenCVFlip:
                // Get the preview data
                NatCam.CaptureFrame(pixelBuffer, false);

                Utils.copyToMat (pixelBuffer, matrix);

                // OpenCV uses an inverted coordinate system. Y-0 is the top of the image, whereas in OpenGL (and so NatCam), Y-0 is the bottom of the image.
                Core.flip (matrix, matrix, 0);

                break;
            case MatCaptureMethod.BlitWithReadPixels:

                // When NatCam.PreviewMatrix function does not work properly. (Zenfone 2)
                // The workaround for a device like this is to use Graphics.Blit with Texture2D.ReadPixels and Texture2D.GetRawTextureData/GetPixels32 to download the pixel data from the GPU.
                // Blit the NatCam preview to a temporary render texture; set the RT active and readback into a Texture2D (using ReadPixels), then access the pixel data in the texture.
                // The texture2D's TextureFormat needs to be RGBA32(Unity5.5+), ARGB32, RGB24, RGBAFloat or RGBAHalf.
                Utils.textureToTexture2D (NatCam.Preview, texture);

                Utils.copyToMat (texture.GetRawTextureData (), matrix);

                // OpenCV uses an inverted coordinate system. Y-0 is the top of the image, whereas in OpenGL (and so NatCam), Y-0 is the bottom of the image.
                Core.flip (matrix, matrix, 0);

                break;
            case MatCaptureMethod.Graphics_CopyTexture:

                // When NatCam.PreviewMatrix function does not work properly. (Zenfone 2)
                if (SystemInfo.copyTextureSupport != UnityEngine.Rendering.CopyTextureSupport.None) {
                    Graphics.CopyTexture (NatCam.Preview, texture);

                    Utils.copyToMat (texture.GetRawTextureData (), matrix);

                    // OpenCV uses an inverted coordinate system. Y-0 is the top of the image, whereas in OpenGL (and so NatCam), Y-0 is the bottom of the image.
                    Core.flip (matrix, matrix, 0);
                } else {
                    if (fpsMonitor != null) {
                        fpsMonitor.consoleText = "SystemInfo.copyTextureSupport: None";
                    }
                    return null;
                }

                break;
            }

            return matrix;
        }

        /// <summary>
        /// Process the image.
        /// </summary>
        /// <param name="matrix">Mat.</param>
        /// <param name="imageProcessingType">ImageProcessingType.</param>
        private void ProcessImage (Mat matrix, ImageProcessingType imageProcessingType = ImageProcessingType.None)
        {
            switch (imageProcessingType) {
            case ImageProcessingType.DrawLine:
                // Draw a diagonal line on our image
                Imgproc.line (matrix, new Point (0, 0), new Point (matrix.cols (), matrix.rows ()), new Scalar (255, 0, 0, 255), 4);

                break;
            case ImageProcessingType.ConvertToGray:
                // Convert a four-channel mat image to greyscale
                if (grayMatrix != null && (grayMatrix.width() != matrix.width() || grayMatrix.height() != matrix.height())) {
                    grayMatrix.Dispose();
                    grayMatrix = null;
                }
                grayMatrix = grayMatrix ?? new Mat(matrix.height(), matrix.width(), CvType.CV_8UC1);

                Imgproc.cvtColor (matrix, grayMatrix, Imgproc.COLOR_RGBA2GRAY);
                Imgproc.cvtColor (grayMatrix, matrix, Imgproc.COLOR_GRAY2RGBA);

                break;
            }
        }

        private void SetMaterials () {
            //Cache the original material
            originalMaterial = preview.materialForRendering;
            //Create the view material
            viewMaterial = new Material(Shader.Find("Hidden/NatCamWithOpenCVForUnity/ImageFlipShader"));
            //Set the raw image material
            preview.material = viewMaterial;
        }

        /// <summary>
        /// Releases all resource.
        /// </summary>
        private void Dispose ()
        {
            NatCam.Release ();

            if (matrix != null) {
                matrix.Dispose ();
                matrix = null;
            }
            if (grayMatrix != null) {
                grayMatrix.Dispose ();
                grayMatrix = null;
            }
            if (texture != null) {
                Texture2D.Destroy (texture);
                texture = null;
            }

            didUpdateThisFrame = false;

            //Reset material
            if (preview) preview.material = originalMaterial;
            //Destroy view material
            Destroy(viewMaterial);
        }

        /// <summary>
        /// Raises the destroy event.
        /// </summary>
        void OnDestroy ()
        {
            Dispose ();
        }

        IEnumerator DetectLampsCoroutine()
        {
            //Color limits initialization
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

            while (true)
            {
                if (matrixForDetection == null)
                {
                    yield return null;
                    continue;
                }

                //Matrix manipulations
                Mat rgbMatrix = new Mat();
                Mat hsvMatrix = new Mat();
                Imgproc.cvtColor(matrixForDetection, rgbMatrix, Imgproc.COLOR_RGBA2RGB);
                Imgproc.cvtColor(rgbMatrix, hsvMatrix, Imgproc.COLOR_RGB2HSV);
                yield return null;

                //Get bright objects
                Mat ThresholdMatrix = new Mat();
                Core.inRange(hsvMatrix, new Scalar(0, 0, 150), new Scalar(180, 255, 255), ThresholdMatrix);
                ThresholdMatrix = morphOps(ThresholdMatrix);

                List<MatOfPoint> Contours = new List<MatOfPoint>();
                Mat Hierarchy = new Mat();
                Imgproc.findContours(ThresholdMatrix, Contours, Hierarchy, Imgproc.RETR_CCOMP, Imgproc.CHAIN_APPROX_SIMPLE);
                yield return null;

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
                yield return null;

                //Create Contour -> ColorPointDictionary
                Dictionary<MatOfPoint, List<Vector2>> ContourColorPointDictionary = new Dictionary<MatOfPoint, List<Vector2>>();
                finalContours = new List<MatOfPoint>();
                finalContours = Contours;

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
                                if (ContourColorPointDictionary.ContainsKey(generalContour))
                                {
                                    if (previousArea < area && previousArea > 0)
                                    {
                                        ContourColorPointDictionary[generalContour][ContourColorPointDictionary[generalContour].Count - 1] = new Vector2((float)(moments.get_m10() / area), (float)(moments.get_m01() / area));
                                    }
                                    else if (previousArea == 0)
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
                yield return null;

                //Get available lamps
                var AvailableLamps = this.GetComponent<LampCommunication>().lampColors;

                if (finalContours.Count == 0 || AvailableLamps.Count == 0)
                {
                    lineProperties = null;
                    yield return null;
                    continue;
                }


                //Fit lines
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
                yield return null;

                //Match lamps!
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

                int permutationIndex = 0;

                //Add IP and send with some DTO
                var DetectedLampsGameObject = GameObject.Find("DetectedLampProperties");
                var DetectedLampsDTO = DetectedLampsGameObject.GetComponent<DetectedLampProperties>().DetectedLamps;

                DetectedLampsDTO.Clear();

                //Dictionary<float, string> functionValueToIPDictionary = new Dictionary<float, string>();

                //Objective Function = f(IP, Contour)
                float[][] objFunction = new float[AvailableLamps.Count()][];

                Dictionary<float, List<Vector2>> functionValueToPointsDictionary = new Dictionary<float, List<Vector2>>();

                for (int l = 0; l < AvailableLamps.Count(); l++)
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
                            endFunction += (((2 * k + 1) / 8) - (Pc - end).magnitude / contourLength) * (((2 * k + 1) / 8) - (Pc - end).magnitude / contourLength);

                            previousStartVector = previousStartVector < (Pc - start).magnitude ? (Pc - start).magnitude : float.MaxValue;
                            previousEndVector = previousEndVector < (Pc - end).magnitude ? (Pc - end).magnitude : float.MaxValue;
                            startVectors.Add((Pc - start).magnitude);
                            endVectors.Add((Pc - end).magnitude);
                        }

                        //DebugText.text = "start + " + string.Join(",", startVectors.Select(x => x.ToString()).ToArray())
                        //    + "\n end " + string.Join(",", endVectors.Select(x => x.ToString()).ToArray());

                        startFunction = previousStartVector == float.MaxValue ? startFunction * 100f : startFunction;
                        endFunction = previousEndVector == float.MaxValue ? endFunction * 100f : endFunction;

                        if (!functionValueToPointsDictionary.ContainsKey(startFunction))
                            functionValueToPointsDictionary.Add(startFunction, new List<Vector2> { end, start });

                        if (!functionValueToPointsDictionary.ContainsKey(endFunction))
                            functionValueToPointsDictionary.Add(endFunction, new List<Vector2> { start, end });

                        objFunValuesForContour.Add(Mathf.Min(startFunction, endFunction));
                    }

                    objFunction[l] = objFunValuesForContour.ToArray();
                }

                yield return null;

                //Get minimum values until no more contours or IPs
                List<int> excludedContourIndex = new List<int>();
                List<int> excludedMacIndex = new List<int>();
                var min = objFunction.SelectMany((subArr, i) => subArr.Select((value, j) => new { i, j, value }))
                    .OrderBy(x => x.value)
                    .ToArray();

                for (int k = 0; k < min.Length; k++)
                {
                    if (excludedMacIndex.Count == AvailableLamps.Count() || excludedContourIndex.Count == finalContours.Count)
                        break;

                    if (!excludedMacIndex.Contains(min[k].i) && !excludedContourIndex.Contains(min[k].j))
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

                        excludedMacIndex.Add(min[k].i);
                        excludedContourIndex.Add(min[k].j);
                    }
                }

                //for (int i = 0; i < AvailableLamps.Count(); i++)
                //{
                //    if (!excludedMacIndex.Contains(i))
                //    {
                //        DetectedLampsGameObject.GetComponent<DetectedLampProperties>().LampMactoLengthDictionary
                //            .Remove(AvailableLamps[i].MacName);
                //    }
                //}
                yield return null;

            }
        }
        
        private static Vector2 TransFormPoint(Vector2 point)
        {
            var ScreenDimensions = NatCam.Camera.PreviewResolution;
            var transFormedPoint = point;
            transFormedPoint.x = Mathf.Max(Mathf.Min(transFormedPoint.x, ScreenDimensions.width), 0f);
            transFormedPoint.y = Mathf.Max(Mathf.Min(transFormedPoint.y, ScreenDimensions.height), 0f);
            transFormedPoint.x = transFormedPoint.x / ScreenDimensions.width;
            transFormedPoint.y = (ScreenDimensions.height - transFormedPoint.y) / ScreenDimensions.height;
            return transFormedPoint;
        }

        /// <summary>
        /// Morphs the ops.
        /// </summary>
        /// <param name="thresh">Thresh.</param>
        private Mat morphOps(Mat thresh)
        {
            //create structuring element that will be used to "dilate" and "erode" image.
            //the element chosen here is a 3px by 3px rectangle
            //Mat erodeElement = Imgproc.getStructuringElement (Imgproc.MORPH_RECT, new Size (3, 3));
            //dilate with larger element so make sure object is nicely visible
            Mat dilateElement = Imgproc.getStructuringElement(Imgproc.MORPH_RECT, new Size(10, 10));

            //Imgproc.erode (thresh, thresh, erodeElement);

            Imgproc.dilate(thresh, thresh, dilateElement);

            return thresh;
        }

        /// <summary>
        /// Raises the back button click event.
        /// </summary>
        public void OnBackButtonClick()
        {
            var DetectedLampsGameObject = GameObject.Find("DetectedLampProperties");
            DetectedLampsGameObject.GetComponent<DetectedLampProperties>().AddLamps = true;
            SceneManager.LoadScene("Main");
            SceneManager.UnloadSceneAsync("VisionCam");
        }

        /// <summary>
        /// Raises the play button click event.
        /// </summary>
        public void OnPlayButtonClick ()
        {
            NatCam.Play ();
        }

        /// <summary>
        /// Raises the pause button click event.
        /// </summary>
        public void OnPauseButtonClick ()
        {
            NatCam.Pause ();
        }

        /// <summary>
        /// Raises the stop button click event.
        /// </summary>
        public void OnStopButtonClick ()
        {
            NatCam.Pause ();
        }

        /// <summary>
        /// Raises the change camera button click event.
        /// </summary>
        public void OnChangeCameraButtonClick ()
        {
			// Switch camera
			NatCam.Pause();
            CameraIndex = (CameraIndex + 1) % DeviceCamera.Cameras.Length;
			didUpdateThisFrame = false;
            NatCam.Camera = DeviceCamera.Cameras[CameraIndex];
			NatCam.Play();         
            //if (NatCam.Camera.IsFrontFacing) NatCam.Camera = DeviceCamera.RearCamera;
            //else NatCam.Camera = DeviceCamera.FrontCamera;
        }

        /// <summary>
        /// Raises the mat capture method dropdown value changed event.
        /// </summary>
        public void OnMatCaptureMethodDropdownValueChanged (int result)
        {
            if ((int)matCaptureMethod != result) {
                matCaptureMethod = (MatCaptureMethod)result;
                if (fpsMonitor != null) {
                    fpsMonitor.consoleText = "";
                }
            }
        }

        /// <summary>
        /// Raises the image processing type dropdown value changed event.
        /// </summary>
        public void OnImageProcessingTypeDropdownValueChanged (int result)
        {
            if ((int)imageProcessingType != result) {
                imageProcessingType = (ImageProcessingType)result;
            }
        }

        /// <summary>
        /// Raises the image flipping method dropdown value changed event.
        /// </summary>
        public void OnImageFlippingMethodDropdownValueChanged (int result)
        {
            if ((int)imageFlippingMethod != result) {
                imageFlippingMethod = (ImageFlippingMethod)result;
            }

            if (imageFlippingMethod == ImageFlippingMethod.Shader) {
                preview.materialForRendering.SetVector("_Mirror", new Vector2(0f, 1f));
            } else {
                preview.materialForRendering.SetVector("_Mirror", Vector2.zero);
            }
        }
    }
        
}