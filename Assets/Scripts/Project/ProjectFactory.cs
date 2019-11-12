using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using VoyagerApp.Lamps;
using VoyagerApp.Videos;
using VoyagerApp.Workspace;
using VoyagerApp.Workspace.Views;

namespace VoyagerApp.Projects
{
    public static class ProjectFactory
    {
        public const string VERSION = "2.1";

        public static ProjectSaveData GetCurrentSaveData()
        {
            var videos = new Video[VideoManager.instance.Videos.Count];

            for (int i = 0; i < videos.Length; i++)
            {
                var video = VideoManager.instance.Videos[i];

                var videoData = new Video
                {
                    guid = video.hash,
                    frames = video.frames,
                    fps = video.fps,
                    url = video.path
                };

                videos[i] = videoData;
            }

            var lamps = new Lamp[LampManager.instance.Lamps.Count];

            for (int i = 0; i < lamps.Length; i++)
            {
                var lamp = LampManager.instance.Lamps[i];
                
                var lampData = new Lamp
                {
                    serial = lamp.serial,
                    length = lamp.pixels,
                    video = lamp.video == null ? "" : lamp.video.hash,
                    address = lamp.address.ToString(),
                    itsh = new float[]
                    {
                        lamp.itshe.i,
                        lamp.itshe.t,
                        lamp.itshe.s,
                        lamp.itshe.h,
                        lamp.itshe.e
                    },
                    mapping = new float[]
                    {
                        lamp.mapping.x1,
                        lamp.mapping.y1,
                        lamp.mapping.x2,
                        lamp.mapping.y2
                    },
                    buffer = lamp.buffer.framesToBuffer
                };

                lamps[i] = lampData;
            }

            var items = new List<Item>();

            foreach(var item in WorkspaceManager.instance.Items)
            {
                if (item is LampItemView lampItem)
                {
                    var itemData = new LampItem
                    {
                        type = "voyager_lamp",
                        serial = lampItem.lamp.serial,
                        position = new float[]
                        {
                            lampItem.position.x,
                            lampItem.position.y
                        },
                        rotation = lampItem.rotation,
                        scale = lampItem.scale
                    };

                    items.Add(itemData);
                }

                if (item is PictureItemView pictureItem)
                {
                    var itemData = new PictureItem
                    {
                        type = "picture",
                        width = pictureItem.picture.width,
                        height = pictureItem.picture.height,
                        data = pictureItem.picture.EncodeToJPG(),
                        order = pictureItem.GetOrder(),
                        position = new float[]
                        {
                            pictureItem.position.x,
                            pictureItem.position.y
                        },
                        rotation = pictureItem.rotation,
                        scale = pictureItem.scale
                    };

                    items.Add(itemData);
                }
            }

            Camera camera = Camera.main;
            var cameraData = new float[]
            {
                camera.transform.position.x,
                camera.transform.position.y,
                camera.orthographicSize
            };

            return new ProjectSaveData
            {
                version = VERSION,
                videos = videos,
                lamps = lamps,
                items = items.ToArray(),
                camera = cameraData
            };
        }

        public static WorkspaceSaveData GetCurrentWorkspaceData()
        {
            var items = new List<Item>();

            foreach (var item in WorkspaceManager.instance.Items)
            {
                if (item is LampItemView lampItem)
                {
                    var itemData = new LampItem
                    {
                        type = "voyager_lamp",
                        serial = lampItem.lamp.serial,
                        position = new float[]
                        {
                            lampItem.position.x,
                            lampItem.position.y
                        },
                        rotation = lampItem.rotation,
                        scale = lampItem.scale
                    };

                    items.Add(itemData);
                }

                if (item is PictureItemView pictureItem)
                {
                    var itemData = new PictureItem
                    {
                        type = "picture",
                        width = pictureItem.picture.width,
                        height = pictureItem.picture.height,
                        data = pictureItem.picture.EncodeToJPG(),
                        order = pictureItem.GetOrder(),
                        position = new float[]
                        {
                            pictureItem.position.x,
                            pictureItem.position.y
                        },
                        rotation = pictureItem.rotation,
                        scale = pictureItem.scale
                    };

                    items.Add(itemData);
                }
            }

            Camera camera = Camera.main;
            var cameraData = new float[]
            {
                camera.transform.position.x,
                camera.transform.position.y,
                camera.orthographicSize
            };

            return new WorkspaceSaveData
            {
                items = items.ToArray(),
                camera = cameraData
            };
        }

        public static IProjectParser GetParser(string json)
        {
            JObject jobj = JObject.Parse(json);
            string version = (string)jobj["version"];

            switch (version)
            {
                case "2.1":
                    return new ProjectParser2_1();
                case "2.0.95":
                case "2.0.96":
                    return new ProjectParser2_0();
            }

            return null;
        }
    }
}
