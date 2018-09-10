using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;
using Voyager.Lamps;

namespace Voyager.Workspace
{
	public class Workspace : MonoBehaviour
    {
		static Workspace instance;
        [SerializeField] GameObject[] SpawnableItems;
		[SerializeField] List<WorkspaceItem> ItemsInWorkspace = new List<WorkspaceItem>();
        
		void Awake()
		{
			instance = this;
		}

		public static void ShowGraphics()
        {
            foreach (WorkspaceItem item in instance.ItemsInWorkspace)
                item.ShowGraphics();
        }

		public static void HideGraphics()
		{
			foreach (WorkspaceItem item in instance.ItemsInWorkspace)
				item.HideGraphics();
		}
              
		public static PhysicalLamp InstantiateLamp(Lamp lamp)
        {
            return InstantiateLamp(lamp, Vector3.zero);
        }

        public static PhysicalLamp InstantiateLamp(Lamp lamp, Vector3 position)
        {
            if (lamp.physicalLamp != null)
            {
                Debug.LogError("Lamp allready in a scene. New position given.");
				DestroyItem(lamp.physicalLamp.GetComponent<WorkspaceItem>());
            }

            GameObject lampPrefab = GetLampPrefab(lamp.Type);
            if (lampPrefab == null)
            {
                Debug.LogError("This type of lamp has no prefab in \"SpawnableItems\" list.");
                return null;
            }

			GameObject physicalObject = InstantiateItem(lampPrefab, position);
            PhysicalLamp physicalLamp = physicalObject.GetComponent<PhysicalLamp>();
            physicalLamp.Setup(lamp);
            lamp.physicalLamp = physicalLamp;

            return physicalLamp;
        }

        public static PhysicalLamp InstantiateLamp(Lamp lamp, Vector3 handle1, Vector3 handle2)
        {
			if (lamp.physicalLamp != null)
			{
                Debug.Log("Lamp allready in a scene. New position given.");
				DestroyItem(lamp.physicalLamp.GetComponent<WorkspaceItem>());
            }
			
            PhysicalLamp physical = InstantiateLamp(lamp);
            physical.GetComponent<LampMove>().SetPosition(handle1, handle2);
            return physical;
        }

		public static Transform InstantiateCamStream()
		{
			return InstantiateItem(GetCamStreamPrefab(), Vector3.forward * 0.7f).transform;
		}

		public static Photo InstantiateImage(Texture2D texture)
		{
			string photoName = (DateTime.Now.ToShortDateString() + "-" + DateTime.Now.ToLongTimeString()).Replace("/", "_").Replace(".", "_").Replace(":", "_");
			return InstantiateImage(texture, Vector3.zero, photoName);
		}

		public static Photo InstantiateImage(Texture2D texture, string photoName)
        {
			return InstantiateImage(texture, Vector3.zero, photoName);
        }

		public static Photo InstantiateImage(Texture2D texture, Vector3 position, string photoName)
		{
			GameObject gameObject = InstantiateItem(GetImagePrefab(), position + Vector3.forward * 0.7f);
            MeshRenderer renderer = gameObject.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>();
            renderer.material = new Material(Shader.Find("Unlit/Texture"));
            renderer.material.mainTexture = texture;
			Photo photo = gameObject.GetComponent<Photo>();
			photo.Setup(renderer.material.mainTexture as Texture2D, photoName);
			return photo;
		}

		public static Video InstantiateVideo(string url, Vector3 position)
		{
			GameObject gameObject = InstantiateItem(GetVideoStreamPrefab(), position + Vector3.forward * 0.7f);
			Video video = gameObject.GetComponent<Video>();
			video.Setup(url);
			return video;
		}

		static GameObject InstantiateItem(GameObject prefab, Vector3 position, WorkspaceItem parent = null)
        {
            GameObject item = Instantiate(prefab, position, Quaternion.identity, instance.transform);
            instance.ItemsInWorkspace.Add(item.GetComponent<WorkspaceItem>());
            return item;
        }

		public static void DestroyItem(WorkspaceItem item)
        {
			if (item.parent != null)
				item.parent.children.Remove(item);

			WorkspaceItem[] clone = item.children.ToArray();

			foreach(WorkspaceItem wi in clone)
				DestroyItem(wi);

            if (item.Type == WorkspaceItem.WorkspaceItemType.Lamp)
                item.GetComponent<PhysicalLamp>().Owner.physicalLamp = null;

            instance.ItemsInWorkspace.Remove(item);
            Destroy(item.gameObject);
        }

        internal static void DestroyLamp(PhysicalLamp physicalLamp)
        {
            physicalLamp.Owner.physicalLamp = null;
            instance.ItemsInWorkspace.Remove(physicalLamp.GetComponent<WorkspaceItem>());
            Destroy(physicalLamp.gameObject);
        }

        public static void Clear()
		{
			while(instance.ItemsInWorkspace.Count != 0)
				DestroyItem(instance.ItemsInWorkspace[0]);
		}

        public static bool ContainsVideoStream()
		{
			foreach (WorkspaceItem item in instance.ItemsInWorkspace)
			{
				if (item.Type == WorkspaceItem.WorkspaceItemType.Cam) return true;
				if (item.Type == WorkspaceItem.WorkspaceItemType.Video) return true;
            }

			return false;
		}

		public static Transform GetVideoSteam()
		{
			foreach (WorkspaceItem item in instance.ItemsInWorkspace)
			{
				if (item.Type == WorkspaceItem.WorkspaceItemType.Cam) return item.transform;
				if (item.Type == WorkspaceItem.WorkspaceItemType.Video) return item.transform;
            }

			return null;
		}

		public static List<WorkspaceItem> GetItemsInWorkspace()
        {
			return instance.ItemsInWorkspace;
        }

		static GameObject GetVideoStreamPrefab()
        {
            foreach (GameObject item in instance.SpawnableItems)
				if (item.GetComponent<WorkspaceItem>().Type == WorkspaceItem.WorkspaceItemType.Video) return item;

            return null;
        }

		static GameObject GetImagePrefab()
		{
			foreach (GameObject item in instance.SpawnableItems)
				if (item.GetComponent<WorkspaceItem>().Type == WorkspaceItem.WorkspaceItemType.Image) return item;

            return null;
		}

        static GameObject GetLampPrefab(LampType type)
        {
            PhysicalLamp physicalLamp;
            foreach (GameObject go in instance.SpawnableItems)
            {
                WorkspaceItem item = go.GetComponent<WorkspaceItem>();
                if(item.Type == WorkspaceItem.WorkspaceItemType.Lamp)
                {
                    physicalLamp = go.GetComponent<PhysicalLamp>();
                    if (physicalLamp.Type == type)
                        return go;
                }
            }
            return null;
        }

		static GameObject GetCamStreamPrefab()
		{
			foreach (GameObject go in instance.SpawnableItems)
            {
                WorkspaceItem item = go.GetComponent<WorkspaceItem>();
				if (item.Type == WorkspaceItem.WorkspaceItemType.Cam)
					return go;
            }
            return null;
		}

		public static Transform GetWorkspaceTransform()
		{
			return instance.transform;
		}

		public static void SaveWorkplace(string filename, Texture2D thumbnailTexture = null)
        {
			if (!Directory.Exists(Application.persistentDataPath + "/images"))
				Directory.CreateDirectory(Application.persistentDataPath + "/images");
			if (!Directory.Exists(Application.persistentDataPath + "/videos"))
                Directory.CreateDirectory(Application.persistentDataPath + "/videos");
			if (!Directory.Exists(Application.persistentDataPath + "/workspaces"))
				Directory.CreateDirectory(Application.persistentDataPath + "/workspaces");

            BinaryFormatter bf = new BinaryFormatter();

			LampSaveData[] lampSaveData = GenerateLampsSaveData();
			ImageSaveData[] imageSaveData = GenerateImagesSaveData();
			VideoSaveData[] videoSaveData = GenerateVideoSaveData();

			string thumbnail = (thumbnailTexture != null) ? SaveThumbnail(thumbnailTexture, filename) : "";

			WorkplaceData data = new WorkplaceData()
			{
				thumbnail = thumbnail,
				lamps = lampSaveData,
				images = imageSaveData,
				//videos = videoSaveData
			};

			FileStream file = File.Create(Application.persistentDataPath + "/workspaces/" + filename + ".dsw");
            bf.Serialize(file, data);
            file.Close();
            file.Dispose();
        }

		static string SaveThumbnail(Texture2D texture, string filename)
		{
			string path = Application.persistentDataPath + "/images/thumb_" + filename + ".png";
			byte[] pngData = texture.EncodeToPNG();
            File.WriteAllBytes(path, pngData);
			return path;
		}

		static LampSaveData[] GenerateLampsSaveData()
		{
			List<Lamp> lampsInWorkplace = new List<Lamp>();
            foreach (WorkspaceItem item in instance.ItemsInWorkspace)
            {
                if (item.Type == WorkspaceItem.WorkspaceItemType.Lamp)
                    lampsInWorkplace.Add(item.GetComponent<PhysicalLamp>().Owner);
            }

            LampSaveData[] lampSaveData = new LampSaveData[lampsInWorkplace.Count];

            for (int i = 0; i < lampsInWorkplace.Count; i++)
            {
                Lamp lamp = lampsInWorkplace[i];
                LampMove lampMove = lamp.physicalLamp.GetComponent<LampMove>();
                Vector3 handle1 = lampMove.sizeHandle1.transform.position;
                Vector3 handle2 = lampMove.sizeHandle2.transform.position;

                LampSaveData lampData = new LampSaveData
                {
                    serial = lamp.Serial,
                    ip = lamp.IP.ToString(),
                    type = (int)lamp.Type,
                    lenght = lamp.Lenght,
                    colordata = lamp.pixelColorData,
                    handle1 = new SerVector3(handle1),
                    handle2 = new SerVector3(handle2)
                };

                lampSaveData[i] = lampData;
            }

			return lampSaveData;
		}

		static ImageSaveData[] GenerateImagesSaveData()
		{
			List<Photo> photosInWorkspace = new List<Photo>();
            foreach (WorkspaceItem item in instance.ItemsInWorkspace)
            {
				if (item.Type == WorkspaceItem.WorkspaceItemType.Image)
					photosInWorkspace.Add(item.GetComponent<Photo>());
            }

			ImageSaveData[] imageSaveData = new ImageSaveData[photosInWorkspace.Count];

			for (int i = 0; i < photosInWorkspace.Count; i++)
			{
				Photo photo = photosInWorkspace[i];
				string path = Application.persistentDataPath + "/images/" + photo.photoName + ".png";
				LampMove move = photo.GetComponent<LampMove>();
				Vector3 handle1 = move.sizeHandle1.transform.position;
				Vector3 handle2 = move.sizeHandle2.transform.position;

				WorkspaceItem item = photo.GetComponent<WorkspaceItem>();
				string[] serials = new string[item.children.Count];
				for (int wi = 0; wi < item.children.Count; wi++)
				{
					Lamp child = item.children[wi].GetComponent<PhysicalLamp>().Owner;
					serials[wi] = child.Serial;
				}

				if (!File.Exists(path))
				{
					FileStream createFile = new FileStream(path, FileMode.Create);
					createFile.Dispose();
				}

				byte[] pngData = photo.texture.EncodeToPNG();
				File.WriteAllBytes(path, pngData);
                Debug.Log(path);

				ImageSaveData imageData = new ImageSaveData
				{
					filepath = path,
					handle1 = new SerVector3(handle1),
					handle2 = new SerVector3(handle2),
					childrenSerials = serials
				};

				imageSaveData[i] = imageData;
			}

			return imageSaveData;
		}

		static VideoSaveData[] GenerateVideoSaveData()
		{
			List<Video> videosInWorkspace = new List<Video>();
			foreach (WorkspaceItem item in instance.ItemsInWorkspace)
            {
				if (item.Type == WorkspaceItem.WorkspaceItemType.Video)
					videosInWorkspace.Add(item.GetComponent<Video>());
            }
			VideoSaveData[] videoSaveData = new VideoSaveData[videosInWorkspace.Count];

			for (int i = 0; i < videosInWorkspace.Count; i++)
			{
				Video video = videosInWorkspace[i];
				string path = Application.persistentDataPath + "/videos/" + video.filename + ".mp4";
				if(path != video.videoUrl)
				{
					byte[] videoData = File.ReadAllBytes(video.videoUrl);
					if (!File.Exists(path))
					{
						FileStream createFile = new FileStream(path, FileMode.Create);
						createFile.Dispose();
					}
					File.WriteAllBytes(path, videoData);
                }

				LampMove move = video.GetComponent<LampMove>();
                Vector3 handle1 = move.sizeHandle1.transform.position;
                Vector3 handle2 = move.sizeHandle2.transform.position;
                
				WorkspaceItem item = video.GetComponent<WorkspaceItem>();
                string[] serials = new string[item.children.Count];
				for (int wi = 0; wi < item.children.Count; wi++)
                {
                    Lamp child = item.children[wi].GetComponent<PhysicalLamp>().Owner;
                    serials[wi] = child.Serial;
                }

				VideoSaveData saveData = new VideoSaveData
				{
					filepath = path,
					handle1 = new SerVector3(handle1),
					handle2 = new SerVector3(handle2),
					videoTime = video.GetVideoTime(),
					childrenSerials = serials
				};

				videoSaveData[i] = saveData;
			}

			return videoSaveData;
		}

		public static void LoadWorkplace()
        {
            string filename = SceneManager.GetActiveScene().name;
            LoadWorkplace(filename);
        }

		public static void LoadWorkplace(string filename)
        {
			if (File.Exists(Application.persistentDataPath + "/workspaces/" + filename + ".dsw"))
            {
                BinaryFormatter bf = new BinaryFormatter();
				FileStream file = File.Open(Application.persistentDataPath + "/workspaces/" + filename + ".dsw", FileMode.Open);
                WorkplaceData data = (WorkplaceData)bf.Deserialize(file);
                file.Close();
                file.Dispose();
                
				LoadLamps(data.lamps);
				LoadImages(data.images);
				//LoadVideos(data.videos);
            }
        }

		static void LoadLamps(LampSaveData[] lampDataArray)
		{         
			LampManager lampManager = LampManager.Instance;

            foreach (LampSaveData lampData in lampDataArray)
            {
                Lamp lamp = null;
                if (lampManager.LampExists(lampData.serial))
					lamp = lampManager.GetLamp(lampData.serial);
                else
                {
                    lamp = new Lamp();
                    lamp.Setup(lampData.serial, lampData.ip, (LampType)lampData.type, lampData.lenght, lampData.colordata);
                    lampManager.GetLamps().Add(lamp);
                }
                InstantiateLamp(lamp, lampData.handle1.ToVector3(), lampData.handle2.ToVector3());
            }
		}

		static void LoadImages(ImageSaveData[] imageDataArray)
		{
			LampManager lampManager = LampManager.Instance;

			foreach(ImageSaveData imageData in imageDataArray)
			{
				if (File.Exists(imageData.filepath))
				{
					byte[] file = File.ReadAllBytes(imageData.filepath);
					Texture2D texture = new Texture2D(2, 2);
					texture.LoadImage(file);
					string photoName = Path.GetFileName(imageData.filepath).Split('.')[0];
					LampMove move = InstantiateImage(texture, photoName).GetComponent<LampMove>();
					move.SetPosition(imageData.handle1.ToVector3(), imageData.handle2.ToVector3());

					WorkspaceItem item = move.GetComponent<WorkspaceItem>();

					foreach (string ssid in imageData.childrenSerials)
						lampManager.GetLamp(ssid).physicalLamp.GetComponent<WorkspaceItem>().SetParent(item);
				}
				else
				{
					Debug.LogError("Saved image not found! " + imageData.filepath);
                }
			}
		}

		static void LoadVideos(VideoSaveData[] videoDataArray)
		{
			LampManager lampManager = LampManager.Instance;

			foreach(VideoSaveData videoData in videoDataArray)
			{
				if (File.Exists(videoData.filepath))
				{
					Video video = InstantiateVideo(videoData.filepath, Vector3.zero);
                    LampMove move = video.GetComponent<LampMove>();
                    move.SetPosition(videoData.handle1.ToVector3(), videoData.handle2.ToVector3());
					video.SetTime(videoData.videoTime);

					WorkspaceItem item = move.GetComponent<WorkspaceItem>();

					foreach (string ssid in videoData.childrenSerials)
                        lampManager.GetLamp(ssid).physicalLamp.GetComponent<WorkspaceItem>().SetParent(item);
				}
				else
					Debug.LogError("Saved video not found! " + videoData.filepath);
			}
		}

		[Serializable]
        public struct WorkplaceData
        {
			public string thumbnail;
            public LampSaveData[] lamps;
			public ImageSaveData[] images;
			public VideoSaveData[] videos;
        }

		[Serializable]
		public struct ImageSaveData
		{
			public string filepath;
			public SerVector3 handle1;
			public SerVector3 handle2;
			public string[] childrenSerials;
		}

		[Serializable]
		public struct VideoSaveData 
		{
			public string filepath;
            public SerVector3 handle1;
            public SerVector3 handle2;
			public float videoTime; // How far the video was last time you saved. // NOT WORKING
			public string[] childrenSerials;
		}
	}
}