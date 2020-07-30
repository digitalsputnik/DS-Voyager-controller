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
        public const string VERSION = "2.4";

        public static ProjectSaveData GetCurrentSaveData()
        {
            var effectList = EffectManager.Effects.Where(e => e is Effects.Video ||
                                                              e is Effects.Image ||
                                                              e is SpoutStream ||
                                                              e is SyphonStream).ToList();
            var effects = new Effect[effectList.Count];

            for (var i = 0; i < effectList.Count; i++)
            {
                var effect = effectList[i];

                switch (effect)
                {
                    case Effects.Video _ when effect.preset:
                        effects[i] = new VideoPreset
                        {
                            id = effect.id,
                            name = effect.name,
                            type = "video_preset"
                        };
                        break;
                    case Effects.Video video:
                        effects[i] = new Video
                        {
                            id = video.id,
                            name = video.name,
                            type = "video",
                            file = video.file,
                            frames = video.frames,
                            fps = video.fps
                        };
                        break;
                    case Effects.Image image:
                        effects[i] = new Image
                        {
                            id = image.id,
                            name = image.name,
                            type = "image",
                            data = image.image.GetRawTextureData()
                        };
                        break;
                    case SyphonStream syphon:
                        effects[i] = new Syphon
                        {
                            type = "syphon",
                            server = syphon.server,
                            application = syphon.application
                        };
                        break;
                    case SpoutStream spout:
                        effects[i] = new Spout
                        {
                            type = "spot",
                            source = spout.source
                        };
                        break;
                }

                effects[i].lift = effect.lift;
                effects[i].contrast = effect.contrast;
                effects[i].saturation = effect.saturation;
                effects[i].blur = effect.blur;
            }

            var lamps = new Lamp[LampManager.instance.Lamps.Count];

            for (var i = 0; i < lamps.Length; i++)
            {
                var lamp = LampManager.instance.Lamps[i];

                var lampData = new Lamp
                {
                    serial = lamp.serial,
                    length = lamp.pixels,
                    effect = lamp.effect == null ? "" : lamp.effect.id,
                    address = lamp.address.ToString(),
                    itsh = new[]
                    {
                        lamp.itshe.i,
                        lamp.itshe.t,
                        lamp.itshe.s,
                        lamp.itshe.h,
                        lamp.itshe.e
                    },
                    mapping = new[]
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
                switch (item)
                {
                    case LampItemView lampItem:
                    {
                        var itemData = new LampItem
                        {
                            type = "voyager_lamp",
                            serial = lampItem.lamp.serial,
                            position = new[]
                            {
                                lampItem.position.x,
                                lampItem.position.y
                            },
                            rotation = lampItem.rotation,
                            scale = lampItem.scale
                        };

                        items.Add(itemData);
                        break;
                    }
                    case PictureItemView pictureItem:
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
                        break;
                    }
                }
            }

            var camera = Camera.main;
            var position = camera.transform.position;
            
            var cameraData = new[]
            {
                position.x,
                position.y,
                camera.orthographicSize
            };

            return new ProjectSaveData
            {
                version = VERSION,
                appVersion = Application.version,
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

            var camera = Camera.main;
            var cameraData = new []
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
                case "2.4":
                    return new ProjectParser2_4();
                case "2.2.2":
                    return new ProjectParser2_2_2(); // HOT FIX VERSION
                case "2.2":
                    return new ProjectParser2_2();
                case "2.1":
                    return new ProjectParser2_1();
                default:
                    return new ProjectParser2_0();
            }
        }
    }
}
