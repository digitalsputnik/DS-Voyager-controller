using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class Ribbon : MonoBehaviour {

	public string IP;
    public string Mac;
	public int Port;
	public int pipeLength;

    public string status;
    public string ssid;
    public string password;

	int pixelCount = 0;
	int startPixel = 0;
	int endPixel = 0;
    public float colorIntensityOffset = 0.3f;

	Pixel currentPixel;
	Material geomMaterial;
	Material glowMaterial;

	//array of arrays representing rgbw colors for each pixel
	byte[,] colorsArray = null;
	private byte[] messageBytes = null;


	byte[] authID = { 0xD5, 0x0A, 0x50, 0x03 };
	byte[] IDbytes = new byte[4];

    string defaultIP = "172.20.0.1";

	public GameObject ColorDataReceiver; 
	GameObject lamp;

    Pixel[] LampPixels;


	void Start()
	{
		//Debug.Log ("Ribbon started....");

		lamp = this.gameObject;
        //lampEndPoint = new IPEndPoint (IPAddress.Parse(IP), 31000);

        //colorsArray = new byte[83, 4];

        //SetupCalibrationTables();
        //StartCoroutine("ApplyColors");

        LampPixels = new Pixel[pipeLength];

        for (int i = 0; i < pipeLength; i++)
        {
            LampPixels[i] = this.gameObject.transform.Find("pixel" + i.ToString()).GetComponent<Pixel>();
        }

	}


	void Update () {

        //Debug.Log(string.Format("Ribbon: Receiving IP {0}", this.IP));
		if (ColorDataReceiver.GetComponent<GetLampColorData> ().colorData.ContainsKey (this.IP) || ColorDataReceiver.GetComponent<GetLampColorData>().colorData.ContainsKey(defaultIP)) {
            //Debug.Log ("Key Found...");
            //Array.Clear (colorsArray, 0, 100);
            //colorsArray = new byte[100, 4];
            //if (messageBytes == null) {
            if (ColorDataReceiver.GetComponent<GetLampColorData>().colorData.ContainsKey(this.IP))
            {
                ColorDataReceiver.GetComponent<GetLampColorData>().colorData.TryGetValue(this.IP, out messageBytes);
            }
            else
            {
                ColorDataReceiver.GetComponent<GetLampColorData>().colorData.TryGetValue(defaultIP, out messageBytes);
            }
			//}

			if (messageBytes.Length == 0) {
				return;
			} 

			pixelCount = 0;
			startPixel = 0;
			endPixel = 0;

			//Debug.Log ("Received Data = "+messageBytes.GetLength(0));

			//byte[] authID = { 0xD5, 0x0A, 0x50, 0x03 };
			//byte[] IDbytes = new byte[4];

			//Parsing
			Array.Copy (messageBytes, 0, IDbytes, 0, 4);

			//check if message has correct ID
			if (ByteArrayCompare (IDbytes, authID)) {
				//Debug.Log ("Message has valid ID...");
				startPixel = BitConverter.ToInt16 (messageBytes, 4);
				//Debug.Log ("StartPixel is: "+startPixel);
				endPixel = BitConverter.ToInt16 (messageBytes, 6);
				//Debug.Log ("EndPixel is: "+endPixel);
				//Debug.Log ("Message has valid ID...");
				if (startPixel < endPixel) {
					pixelCount = endPixel - startPixel;
				} else {
					pixelCount = startPixel - endPixel;
				}
				//Debug.Log ("PixelCount is: "+pixelCount);

				//array of arrays representing rgbw colors for each pixel
				colorsArray = new byte[pixelCount, 4];
				//Debug.Log ("Size of colorArray is: "+colorsArray.GetLength(0));

				for (int i = 0; i < pixelCount; i++) {
					colorsArray [startPixel + i, 0] = messageBytes [8 + (i * 4)];
					//Debug.Log ("colorsArray[i][0] is: " + colorsArray [i,0]);
					colorsArray [startPixel + i, 1] = messageBytes [9 + (i * 4)];
					//Debug.Log ("colorsArray[i][1] is: " + colorsArray [i,1]);
					colorsArray [startPixel + i, 2] = messageBytes [10 + (i * 4)];
					//Debug.Log ("colorsArray[i][2] is: " + colorsArray [i,2]);
					colorsArray [startPixel + i, 3] = messageBytes [11 + (i * 4)];
					//Debug.Log ("colorsArray[i][3] is: " + colorsArray [i,3]);
				}

				//Array.Clear (messageBytes, 0, pixelCount);
				//Array.Resize (ref messageBytes, 0); 
				//Debug.Log ("[3] \"IP: " + IP + " - Finished reading data at " + Time.time.ToString ());

			} else {
				//Debug.Log ("Incorrect messageID... Yielding control at " + Time.time.ToString ());
				return;
			}

			//apply colors to scene lamp
			if (pixelCount > 0) {
				//Debug.Log ("Starting to apply colors");
				Color pixelColor;
				for (int i = 0; i < pixelCount; i++) {
					// create unity color
                    
					pixelColor = new Color (Mathf.Max ((float)colorsArray [i, 0] / 255.0f, (float)colorsArray [i, 3] / 255.0f), Mathf.Max ((float)colorsArray [i, 1] / 255.0f, (float)colorsArray [i, 3] / 255.0f), Mathf.Max ((float)colorsArray [i, 2] / 255.0f, (float)colorsArray [i, 3] / 255.0f), 0f);
                    //Debug.Log ("Pixel color is: "+pixelColor);
                    pixelColor.r = pixelColor.r * (1 - colorIntensityOffset) + Mathf.Ceil(pixelColor.r) * colorIntensityOffset;
                    pixelColor.g = pixelColor.g * (1 - colorIntensityOffset) + Mathf.Ceil(pixelColor.g) * colorIntensityOffset;
                    pixelColor.b = pixelColor.b * (1 - colorIntensityOffset) + Mathf.Ceil(pixelColor.b) * colorIntensityOffset;
                    pixelColor.a = pixelColor.a * (1 - colorIntensityOffset) + Mathf.Ceil(pixelColor.a) * colorIntensityOffset;
                    //find child pixels and apply the color created above.
                    //TODO: Replace all find functions!!
                    if (startPixel < endPixel) {
						try {
                            //currentPixel = this.gameObject.transform.Find ("pixel" + (startPixel + i)).GetComponent<Pixel> ();
                            currentPixel = LampPixels[startPixel + i];
                        } catch (Exception) {
							Debug.Log ("Error on loading colors!");
							Debug.Log (startPixel);
							Debug.Log (pixelCount);
							Debug.Log (IP);
						} finally {
                            //currentPixel = this.gameObject.transform.Find ("pixel" + (startPixel + i)).GetComponent<Pixel> ();
                            currentPixel = LampPixels[startPixel + i];
                        }

					} else {
                        //currentPixel = this.gameObject.transform.Find ("pixel" + (startPixel - i)).GetComponent<Pixel> ();
                        currentPixel = LampPixels[startPixel + i];
                    }
					geomMaterial = currentPixel.transform.Find ("LEDmodule").GetComponent<Renderer> ().material;
					glowMaterial = currentPixel.transform.Find ("glow").GetComponent<Renderer> ().material;
					geomMaterial.color = pixelColor;
					glowMaterial.SetColor ("_Color", new Color (pixelColor.r, pixelColor.g, pixelColor.b, 0.3f));

				}
			}
			colorsArray = null;
			//Array.Clear (colorsArray, 0, 100);
			messageBytes = null;
		}


	}


/*	void OnDestroy()
	{
		killThread = true;
	}
*/

    public void Restart() {
		//remoteEndPoint = new IPEndPoint(IPAddress.Parse(IP), Port);
	} 


	static bool ByteArrayCompare(byte[] a1, byte[] a2)
	{
		if (a1.Length != a2.Length)
			return false;

		for (int i=0; i<a1.Length; i++)
			if (a1[i]!=a2[i])
				return false;

		return true;
	}


	// Unity Application Quit Function
/*	void OnApplicationQuit()
	{
		stopThread();
	}

	// Stop reading UDP messages
	private void stopThread()
	{
		killThread = true;
		if (receiveThread.IsAlive)
		{
			receiveThread.Abort();
		}
		receivingUDPClient.Close();
	}


	void ReceiveData() {

		Debug.Log ("Thread started....");

		lampEndPoint = new IPEndPoint (IPAddress.Parse(IP), 31000);
		byte[] ReceivedMessageBytes;
		if (receivingUDPClient == null) {
			receivingUDPClient = animSender.LampCommunicationClient;
		}
		//receivingUDPClient.Client.Blocking = false;
		receivingUDPClient.Client.ReceiveTimeout = 250;
		receivingUDPClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
		receivingUDPClient.Client.ReceiveBufferSize = 1024;
		receivingUDPClient.Send(new byte[] { }, 0, new IPEndPoint(IPAddress.Parse(IP), 31000));

		//colorsArray = new byte[100, 4];

		while (true) 
		{
			if (receivingUDPClient.Available > 0) {
				//Debug.Log ("[2] IP: " + IP + " - Data available...");


				ReceivedMessageBytes = receivingUDPClient.Receive (ref lampEndPoint);
				//Debug.Log ("Received Data = " + ReceivedMessageBytes.GetLength (0));

				if (ReceivedMessageBytes.Length == 0) {
					return;
				}
				//ready = false;
				//Debug.Log ("Received Data = "+ReceivedMessageBytes.GetLength(0));
				//Debug.Log ("IP: " + IP + " -  Received data at " + Time.time.ToString ());

			
				byte[] authID = { 0xD5, 0x0A, 0x50, 0x03 };
				byte[] IDbytes = new byte[4];

				//Parsing
				Array.Copy (ReceivedMessageBytes, 0, IDbytes, 0, 4);

				//check if message has correct ID
				if (ByteArrayCompare (IDbytes, authID)) {
					//Debug.Log ("Message has valid ID...");
					startPixel = BitConverter.ToInt16 (ReceivedMessageBytes, 4);
					//Debug.Log ("StartPixel is: "+startPixel);
					endPixel = BitConverter.ToInt16 (ReceivedMessageBytes, 6);
					//Debug.Log ("EndPixel is: "+endPixel);
					//Debug.Log ("Message has valid ID...");
					if (startPixel < endPixel) {
						pixelCount = endPixel - startPixel;
					} else {
						pixelCount = startPixel - endPixel;
					}
					//Debug.Log ("PixelCount is: "+pixelCount);

					//array of arrays representing rgbw colors for each pixel
					//colorsArray = new byte[pixelCount, 4];
					//Debug.Log ("Size of colorArray is: "+colorsArray.GetLength(0));

					for (int i = 0; i < pixelCount; i++) {
						colorsArray [startPixel + i, 0] = ReceivedMessageBytes [8 + (i * 4)];
						//Debug.Log ("colorsArray[i][0] is: " + colorsArray [i,0]);
						colorsArray [startPixel + i, 1] = ReceivedMessageBytes [9 + (i * 4)];
						//Debug.Log ("colorsArray[i][1] is: " + colorsArray [i,1]);
						colorsArray [startPixel + i, 2] = ReceivedMessageBytes [10 + (i * 4)];
						//Debug.Log ("colorsArray[i][2] is: " + colorsArray [i,2]);
						colorsArray [startPixel + i, 3] = ReceivedMessageBytes [11 + (i * 4)];
						//Debug.Log ("colorsArray[i][3] is: " + colorsArray [i,3]);
					}

					Array.Clear (ReceivedMessageBytes, 0, pixelCount);
					Array.Resize (ref ReceivedMessageBytes, 0); 
					//Debug.Log ("[3] \"IP: " + IP + " - Finished reading data at " + Time.time.ToString ());

				} else {
					//Debug.Log ("Incorrect messageID... Yielding control at " + Time.time.ToString ());
					return;
				}
				ready = true;
				//colorsArray = null;
				//yield return null;

			} else {
				//Debug.Log ("No data available...");
				//ready = false;
			}

			if (killThread)
				break;
			
		}
		//myClient.Close();
		//Debug.Log("[6] IP: "+IP+" - Yielding control at "+Time.time.ToString());

	} */


/*	void Update ()
	{

		//apply colors to scene lamp
		if (ready) {
			//Debug.Log ("[4] IP: " + IP + " -  Starting to apply at " + Time.time.ToString ());
			//Debug.Log ("Starting to apply colors");
			Debug.Log (lamp.name);

			Color pixelColor;

			for (int i = 0; i < pixelCount; i++) {
				// create unity color
				pixelColor = new Color (Mathf.Max ((float)colorsArray [startPixel+i, 0] / 255.0f, (float)colorsArray [startPixel+i, 3] / 255.0f), Mathf.Max ((float)colorsArray [startPixel+i, 1] / 255.0f, (float)colorsArray [startPixel+i, 3] / 255.0f), Mathf.Max ((float)colorsArray [startPixel+i, 2] / 255.0f, (float)colorsArray [startPixel+i, 3] / 255.0f), 0f);
				//Debug.Log ("Pixel color is: "+pixelColor);

				//find child pixel and apply the color created above.
				if (startPixel < endPixel) {
					try {
						currentPixel = lamp.transform.Find ("pixel" + (startPixel + i)).GetComponent<Pixel> ();
					} catch (Exception) {
						Debug.Log ("Error on loading colors!");
						Debug.Log (startPixel);
						Debug.Log (pixelCount);
						Debug.Log (IP);
					} finally {
						currentPixel = lamp.transform.Find ("pixel" + (startPixel + i)).GetComponent<Pixel> ();
					}

				} else {
					currentPixel = lamp.transform.Find ("pixel" + (startPixel - i)).GetComponent<Pixel> ();
				}
				geomMaterial = currentPixel.transform.Find ("LEDmodule").GetComponent<Renderer> ().material;
				glowMaterial = currentPixel.transform.Find ("glow").GetComponent<Renderer> ().material;
				geomMaterial.color = pixelColor;
				glowMaterial.SetColor ("_Color", new Color (pixelColor.r, pixelColor.g, pixelColor.b, 0.3f));

			}
			//Debug.Log ("[5] IP: " + IP + " -  Finsihed applying colors at " + Time.time.ToString ());
			pixelCount = 0;
			ready = false;
			//Array.Clear (colorsArray, 0, 100);
		} else {
			//Debug.Log ("PixelCount is 0... Yielding control at " + Time.time.ToString ());
			return;
		}
	}

	*/

    [Range(1,30)]
	public int UpdateFps;

	//83 long voyager with 4 bytes each value
	byte[] lightValues = new byte[332];
	int[] WhiteCalibrationTemperatureNodes;
	int[][] WhiteCalibrationTable;
	int[][] HueCalibrationTable;

/*
	IPEndPoint remoteEndPoint;
	UdpClient client;

	IPEndPoint lampEndPoint;
	UdpClient myClient;


	//colorsArray
	byte[,] colorsArray;

    private Thread sendThread;
	private Thread readThread;

	// Use this for initialization
	void Start () {
		//remoteEndPoint = new IPEndPoint(IPAddress.Parse(IP), Port);
		//client = new UdpClient();

        //NOTE: Unnecessary
        //fill the array with 0's
        //for(int i=0; i<332; i++) 
        //	lightValues [i] = 0x00;

        SetupCalibrationTables();

        //Start polling lamp for Color Data
        //thisClient.EnableBroadcast = true;

        StartCoroutine("GetLampColorData");

        //start read thread
        //readThread = new Thread(new ThreadStart(GetLampColorData));
        //readThread = new Thread(new ThreadStart(dataSender));//new ThreadStart(GetLampColorData));
        //readThread.IsBackground = true;
        //readThread.Start();

    }

*/







/*

	// receive color data from lamp
	IEnumerator GetLampColorData()
	{
        //Debug.Log ("Inside coroutine!");

		lampEndPoint = new IPEndPoint(IPAddress.Any, 31000);

        myClient = animSender.LampCommunicationClient;
		//myClient.Client.ExclusiveAddressUse = false;
		//myClient.Client.Bind (lampEndPoint);
        //myClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
		myClient.Client.ReceiveTimeout = 250;

        //myClient.Client.SetSocketOption(SocketOptionLevel.Udp, SocketOptionName.NoChecksum, true);
        myClient.Client.ReceiveBufferSize = 1024;
		myClient.Client.Blocking = false;
		myClient.Send(new byte[] { }, 0, new IPEndPoint(IPAddress.Parse(IP), 31000));

        //try{
        //	myClient.Connect(lampEndPoint);
        //}
        //catch (Exception e ) {
        //	Debug.Log("Exception:   "+e.ToString());
        //}
        byte[] ReceivedMessageBytes;
		Pixel currentPixel;
		Material geomMaterial;
		Material glowMaterial;

		while (true)
		{
			//Debug.Log("[1] IP: "+IP+" - started running at "+Time.time.ToString());
            
            
            int pixelCount = 0;
			int startPixel = 0;
			int endPixel = 0;

			if (myClient.Available > 0) 
			{
				Debug.Log("[2] IP: "+IP+" - Data available...");
				pixelCount = 0;
				startPixel = 0;
				endPixel = 0;

                ReceivedMessageBytes = this.myClient.Receive(ref this.lampEndPoint);

				if (ReceivedMessageBytes.Length == 0 || lampEndPoint.Address.ToString() != IP)
                {
                    //myClient.Close();
					Debug.Log("IP: "+IP+" - Nothing to read.... Yielding control at "+Time.time.ToString());
					yield return null;
					//Debug.Log("Continuing at "+Time.time.ToString());
                    continue;
                }
                
				//Debug.Log ("Received Data = "+ReceivedMessageBytes.GetLength(0));
				//Debug.Log ("IP: "+IP+" -  Received data at " + Time.time.ToString ());

				byte[] authID = { 0xD5, 0x0A, 0x50, 0x03 };
				byte[] IDbytes = new byte[4];

				//Parsing
				Array.Copy(ReceivedMessageBytes, 0, IDbytes, 0, 4);

				//check if message has correct ID
				if (ByteArrayCompare (IDbytes, authID)) {
					//Debug.Log ("Message has valid ID...");
					startPixel = BitConverter.ToInt16 (ReceivedMessageBytes, 4);
					//Debug.Log ("StartPixel is: "+startPixel);
					endPixel = BitConverter.ToInt16 (ReceivedMessageBytes, 6);
					//Debug.Log ("EndPixel is: "+endPixel);
					//Debug.Log ("Message has valid ID...");
					if (startPixel < endPixel) {
						pixelCount = endPixel - startPixel;
					} else {
						pixelCount = startPixel - endPixel;
					}
					//Debug.Log ("PixelCount is: "+pixelCount);

					//array of arrays representing rgbw colors for each pixel
					colorsArray = new byte[pixelCount, 4];
					//Debug.Log ("Size of colorArray is: "+colorsArray.GetLength(0));

					for (int i = 0; i < pixelCount; i++) {
						colorsArray [i, 0] = ReceivedMessageBytes [8 + (i * 4)];
						//Debug.Log ("colorsArray[i][0] is: " + colorsArray [i,0]);
						colorsArray [i, 1] = ReceivedMessageBytes [9 + (i * 4)];
						//Debug.Log ("colorsArray[i][1] is: " + colorsArray [i,1]);
						colorsArray [i, 2] = ReceivedMessageBytes [10 + (i * 4)];
						//Debug.Log ("colorsArray[i][2] is: " + colorsArray [i,2]);
						colorsArray [i, 3] = ReceivedMessageBytes [11 + (i * 4)];
						//Debug.Log ("colorsArray[i][3] is: " + colorsArray [i,3]);
					}

					Array.Clear (ReceivedMessageBytes, 0, pixelCount);
					Array.Resize (ref ReceivedMessageBytes, 0); 
					Debug.Log("[3] \"IP: "+IP+" - Finished reading data at "+Time.time.ToString());

				} else {
					Debug.Log("Incorrect messageID... Yielding control at "+Time.time.ToString());
					yield return null;
					continue;
				}

				//apply colors to scene lamp
				if (pixelCount > 0) {
					Debug.Log ("[4] IP: "+IP+" -  Starting to apply at " + Time.time.ToString ());
					//Debug.Log ("Starting to apply colors");
					Color pixelColor;
					for (int i = 0; i < pixelCount; i++) {
						// create unity color
						pixelColor = new Color (Mathf.Max ((float)colorsArray [i, 0] / 255.0f, (float)colorsArray [i, 3] / 255.0f), Mathf.Max ((float)colorsArray [i, 1] / 255.0f, (float)colorsArray [i, 3] / 255.0f), Mathf.Max ((float)colorsArray [i, 2] / 255.0f, (float)colorsArray [i, 3] / 255.0f), 0f);
						//Debug.Log ("Pixel color is: "+pixelColor);

						//find child pixels and apply the color created above.
						if (startPixel < endPixel) {
							try {
								currentPixel = this.gameObject.transform.Find ("pixel" + (startPixel + i)).GetComponent<Pixel> ();
							} catch (Exception) {
								Debug.Log ("Error on loading colors!");
								Debug.Log (startPixel);
								Debug.Log (pixelCount);
								Debug.Log (IP);
							} finally {
								currentPixel = this.gameObject.transform.Find ("pixel" + (startPixel + i)).GetComponent<Pixel> ();
							}
                            
						} else {
							currentPixel = this.gameObject.transform.Find ("pixel" + (startPixel - i)).GetComponent<Pixel> ();
						}
						geomMaterial = currentPixel.transform.Find ("LEDmodule").GetComponent<Renderer> ().material;
						glowMaterial = currentPixel.transform.Find ("glow").GetComponent<Renderer> ().material;
						geomMaterial.color = pixelColor;
						glowMaterial.SetColor ("_Color", new Color (pixelColor.r, pixelColor.g, pixelColor.b, 0.3f));

					}
					Debug.Log ("[5] IP: "+IP+" -  Finsihed applying colors at " + Time.time.ToString ());
					pixelCount = 0;
				} else {
					Debug.Log("PixelCount is 0... Yielding control at "+Time.time.ToString());
					yield return null;
					continue;
				}
				colorsArray = null;
                //yield return null;
            }
            //myClient.Close();
			//Debug.Log("[6] IP: "+IP+" - Yielding control at "+Time.time.ToString());
            yield return null;
		}
	}

	*/

	/*
	//send thread function
	private void dataSender() {
		byte[] data = new byte[343];
        //header
        //RGBW
        System.Buffer.BlockCopy(new byte[] { 0xD5, 0x0A, 0x10, 0x03 }, 0, data, 0, 4);
        //ITSH
        //System.Buffer.BlockCopy(new byte[] { 0xD5, 0x0A, 0x10, 0x04 }, 0, data, 0, 4);
        //short voyager
        if (pipeLength == 0) {
			//start & finish pixels
			System.Buffer.BlockCopy (new byte[]{ 0x00, 0x00, 0x00, 0x27 }, 0, data, 4, 4);
			//terminator + empty checksum
			System.Buffer.BlockCopy (new byte[]{ 0xEF, 0xFE, 0x00 }, 0, data, 164, 3);
		}
		//long voyager
		if (pipeLength == 1) {
			//start & finish pixels
			System.Buffer.BlockCopy (new byte[]{ 0x00, 0x00, 0x00, 0x52 }, 0, data, 4, 4);
			//terminator + empty checksum
			System.Buffer.BlockCopy (new byte[]{ 0xEF, 0xFE, 0x00 }, 0, data, 340, 3);
		}

		//main loop for data sending
		while (true) {
            //rgb data
            client = new UdpClient();
            remoteEndPoint = new IPEndPoint(IPAddress.Parse(IP), Port);
            if (pipeLength == 0) {
				//copy data to pixel values
				System.Buffer.BlockCopy (lightValues, 0, data, 8, 156);
				client.Send (data, 167, remoteEndPoint);
			}
			if (pipeLength == 1) {
				//copy data to pixel values
				System.Buffer.BlockCopy (lightValues, 0, data, 8, 332);
				client.Send (data, data.Length, remoteEndPoint);
			}
            client.Close();
            if (killThread)
                break;

            Thread.Sleep ((int)(1.0f / UpdateFps * 1000));
		}
	}




	public void setPixel(int pix, float r, float g, float b, float w) {
		lightValues [pix * 4] = Convert.ToByte((int)(r*255));
		lightValues [pix * 4+1] = Convert.ToByte((int)(g*255));
		lightValues [pix * 4+2] = Convert.ToByte((int)(b*255));
		lightValues [pix * 4+3] = Convert.ToByte((int)(w*255));
	}
	*/

	public void setPixel(int pix, Vector4 ITSHin) {

        /*
        //Light fixture specific Convert ITSH to RGBW 
        Vector4 finalColor = new Vector4(0,0,0,0);
		finalColor = ITSHin.x * new Vector4(1,0.75f,0.3f,1); //fixed 3200K white
		//if there is saturation
		if (ITSHin.z > 0) {
			Color hue = Color.HSVToRGB (ITSHin.w, 1f, ITSHin.x);
			hue = hue * ITSHin.z;
			finalColor = finalColor * (1f-ITSHin.z);
			//merge wb and color
			finalColor.x = finalColor.x + hue.r;
			finalColor.y = finalColor.y + hue.g;
			finalColor.z = finalColor.z + hue.b;
		}
		//clamp values
		finalColor.x = Mathf.Clamp(finalColor.x,0.0f,1.0f);
		finalColor.y = Mathf.Clamp(finalColor.y,0.0f,1.0f);
		finalColor.z = Mathf.Clamp(finalColor.z,0.0f,1.0f);
		finalColor.w = Mathf.Clamp(finalColor.w,0.0f,1.0f);
		//set values to output array
		lightValues [pix * 4]   = Convert.ToByte((int)(finalColor.x*255));
		lightValues [pix * 4+1] = Convert.ToByte((int)(finalColor.y*255));
		lightValues [pix * 4+2] = Convert.ToByte((int)(finalColor.z*255));
		lightValues [pix * 4+3] = Convert.ToByte((int)(finalColor.w*255));
		*/
        //ITSH output
        //lightValues [pix * 4]   = Convert.ToByte((int)(ITSHin.x*255));
        //lightValues [pix * 4+1] = Convert.ToByte((int)(ITSHin.y*255));
        //lightValues [pix * 4+2] = Convert.ToByte((int)(ITSHin.z*255));
        //lightValues [pix * 4+3] = Convert.ToByte((int)(ITSHin.w*255));

        //ITSH to RGBW conversion
        int[] RGBW = ITSHtoRGBW(ITSHin);
        for (int c = 0; c < 4; c++)
        {
            lightValues[pix * 4+c] = Convert.ToByte(RGBW[c]);
        }
    } 

    //Color calibration and converstion from ITSH to RGBW
    void SetupCalibrationTables()
    {
        WhiteCalibrationTemperatureNodes = new int[] { 0, 7710, 13107, 20817, 25445, 28527, 65535, 65535 };

        WhiteCalibrationTable = new int[][]
        {
            new int[] {255, 39, 0, 1},	 // 1500 K						// ToDo: needs some CCT tweaking
		    new int[] {213, 80, 0, 64}, 	 // 2500 K						// "
		    new int[] {0, 0, 0, 255},	     // 3200 K ~ 3100...3200 K		// "
		    new int[] {0, 152, 128, 255},	 // 4000 K ~ 4200...4300 K		// "
		    new int[] {0, 222, 199, 255},  // 4800 K ~ 4900 K				// "
		    new int[] {0, 255, 255, 255},	 // 5600 K ~ 5200 K				// "
		    new int[] {0, 171, 255, 105},  // 10000 K
		    new int[] {0, 171, 255, 105} 	 // 10000 K aux
        };

        HueCalibrationTable = new int[][]
        {
            new int[] {255, 0, 0},	// 0	<---- Red
		    new int[] {255, 4, 0},	// 10
		    new int[] {255, 16, 0},	// 20
		    new int[] {255, 64, 0},	// 30
		    new int[] {255, 128, 0},	// 40
		    new int[] {255, 192, 0},	// 50
		    new int[] {255, 255, 0},	// 60	<---- Yellow
		    new int[] {192, 255, 0},	// 70
		    new int[] {128, 255, 0},	// 80
		    new int[] {64, 255, 0},	// 90
		    new int[] {16, 255, 0},	// 100
		    new int[] {4, 255, 0},	// 110
		    new int[] {0, 255, 0},	// 120	<---- Green
		    new int[] {0, 255, 4},	// 130
		    new int[] {0, 255, 16},	// 140
		    new int[] {0, 255, 64},	// 150
		    new int[] {0, 255, 128},	// 160
		    new int[] {0, 255, 192},	// 170
		    new int[] {0, 255, 255},	// 180	<---- Cyan
		    new int[] {0, 192, 255},	// 190
		    new int[] {0, 128, 255},	// 200
		    new int[] {0, 64, 255},	// 210
		    new int[] {0, 16, 255},	// 220
		    new int[] {0, 4, 255},	// 230
		    new int[] {0, 0, 255},	// 240	<---- Blue
		    new int[] {4, 0, 255},	// 250
		    new int[] {16, 0, 255},	// 260
		    new int[] {64, 0, 255},	// 270
		    new int[] {128, 0, 255},	// 280
		    new int[] {192, 0, 255},	// 290
		    new int[] {255, 0, 255},	// 300	<---- Magenta
		    new int[] {255, 0, 192},	// 310
		    new int[] {255, 0, 128},	// 320
		    new int[] {255, 0, 64},	// 330
		    new int[] {255, 0, 16},	// 340
		    new int[] {255, 0, 4},	// 350
		    // Auxiliary
		    new int[] {255, 0, 0},	// 360  <---- Red (auxiliary wrap)
        };
    }

    public int[] ITSHtoRGBW(Vector4 ITSH)
    {
        if(WhiteCalibrationTable == null || WhiteCalibrationTemperatureNodes == null || HueCalibrationTable == null)
            SetupCalibrationTables();

        //T should be 0-65535 ~ 1500K - 10000K
        ITSH.y = (ITSH.y - 0.15f)*(10000f/8500f);
        
        //Vector conversion
        Vector4 ITSH_16 = ITSH * 65535;
        int i = 0;
        int interpolation = 0;

        //White balance
        int[] WB_RGBW = new int[] {0,0,0,0};

        int T = (int)ITSH_16.y;
        for (i = 0; i < WhiteCalibrationTemperatureNodes.Length - 2; i++)
        {
            if (T >= WhiteCalibrationTemperatureNodes[i] && T < WhiteCalibrationTemperatureNodes[i + 1])
            {
                //i = index;
                break;
            }
        }

        if (WhiteCalibrationTemperatureNodes[i + 1] == WhiteCalibrationTemperatureNodes[i])
        {
            interpolation = 0;
        }
        else
        {
            interpolation = ((10000 * (T - WhiteCalibrationTemperatureNodes[i]) / (WhiteCalibrationTemperatureNodes[i + 1] - WhiteCalibrationTemperatureNodes[i])));
        }
        

        for (int c = 0; c < 4; c++)
        {
            WB_RGBW[c] = WhiteCalibrationTable[i][c] + (WhiteCalibrationTable[i + 1][c] - WhiteCalibrationTable[i][c]) * interpolation / 10000;
        }

        //Hue
        int[] HS_RGBW = new int[] { 0, 0, 0, 0 };
        int H = (int)ITSH_16.w;

        for (i = 0; i < HueCalibrationTable.Length - 2; i++)
        {
            if (H / 1820 >= i && H / 1820 < i + 1)
            {
                //i = index;
                break;
            }
        }

        interpolation = (H - i * 1820) * 10000 / 1820;
        //NOTE: For some odd reason, this was c < 3
        for (int c = 0; c < 3; c++)
        {
            HS_RGBW[c] = HueCalibrationTable[i][c] + (HueCalibrationTable[i + 1][c] - HueCalibrationTable[i][c]) * interpolation / 10000;
        }

        //Saturation (Blend between White Balance and Hue)
        int[] Blend_RGBW = new int[] { 0, 0, 0, 0 };
        int S = (int)ITSH_16.z;

        for (int c = 0; c < 4; c++)
        {
            Blend_RGBW[c] = HS_RGBW[c] * S / 65535 + WB_RGBW[c] * (65535 - S) / 65535;
        }

        //Intensity
        int I = (int)ITSH_16.x;
        for (int c = 0; c < 4; c++)
        {
            Blend_RGBW[c] = Blend_RGBW[c] * I / 65535;
        }

        //Correction for overflow
        for (int c = 0; c < 4; c++)
        {
            Blend_RGBW[c] = Mathf.Min(255, Math.Max(0, Blend_RGBW[c]));
        }

        return Blend_RGBW;
    }
    
}
