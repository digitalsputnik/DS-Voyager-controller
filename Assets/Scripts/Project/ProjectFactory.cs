using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;
using VoyagerApp.Effects;
using VoyagerApp.Lamps;
using VoyagerApp.Workspace;
using VoyagerApp.Workspace.Views;

namespace VoyagerApp.Projects
{
    public static class ProjectFactory
    {
        public const string VERSION = "2.2";

        public static ProjectSaveData GetCurrentSaveData()
        {
            var effectList = EffectManager.Effects.Where(e => e is Effects.Video).ToList();
            var effects = new Effect[effectList.Count];

            for (int i = 0; i < effectList.Count; i++)
            {
                var effect = effectList[i];

                if (effect is Effects.Video video)
                {
                    if (effect.preset)
                    {
                        var videoData = new VideoPreset
                        {
                            id = effect.id,
                            name = effect.name,
                            type = "video_preset"
                        };

                        effects[i] = videoData;
                    }
                    else
                    {
                        var videoData = new Video
                        {
                            id = video.id,
                            name = video.name,
                            type = "video",
                            file = video.file,
                            frames = video.frames,
                            fps = video.fps
                        };

                        effects[i] = videoData;
                    }
                }
            }

            var lamps = new Lamp[LampManager.instance.Lamps.Count];

            for (int i = 0; i < lamps.Length; i++)
            {
                var lamp = LampManager.instance.Lamps[i];

                var lampData = new Lamp
                {
                    serial = lamp.serial,
                    length = lamp.pixels,
                    effect = lamp.effect == null ? "" : lamp.effect.id,
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
                        lamp.mapping.p1.x,
                        lamp.mapping.p1.y,
                        lamp.mapping.p2.x,
                        lamp.mapping.p2.y
                    },
                    buffer = lamp.buffer.frames
                };

                lamps[i] = lampData;
            }

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

            return new ProjectSaveData
            {
                version = VERSION,
                effects = effects,
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
                case "2.2":
                    return new ProjectParser2_2();
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
