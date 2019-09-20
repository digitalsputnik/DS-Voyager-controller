using System;
using System.Linq;
using UnityEngine;
using VoyagerApp.Lamps;

namespace VoyagerApp.Workspace.Views
{
    public class LampItemView : WorkspaceItemView
    {
        [SerializeField] protected TextMesh nameText    = null;
        [SerializeField] Color normalTextColor          = Color.black;
        [SerializeField] internal Color selectedTextColor = Color.black;

        public Lamp lamp;
        string info = "";
        bool prevCon;

        void Update()
        {
            if (prevCon != lamp.connected)
            {
                prevCon = lamp.connected;
                RedrawText();
            }
        }

        public bool Selected { get; private set; }

        public override void Setup(object data)
        {
            lamp = (Lamp)data;
            nameText.text = lamp.serial;
            normalTextColor = nameText.color;
            base.Setup(data);
            RedrawText();
        }

        public void SetInfo(string i)
        {
            info = i;
            RedrawText();
        }

        public virtual void Select()
		{
            nameText.color = selectedTextColor;
            Selected = true;
		}

        public virtual void Deselect()
		{
            nameText.color = normalTextColor;
            Selected = false;
		}

        public void RedrawText()
        {
            nameText.text = info == "" ? mainName : $"{mainName}, {info}";
        }

        public virtual Vector2[] PixelWorldPositions() { return null; }

        public virtual void PushColors(Color32[] colors, long frame) { }

        public override WorkspaceItemSaveData ToData()
        {
            return new LampItemSaveData
            {
                guid = guid,
                x = position.x,
                y = position.y,
                scale = scale,
                rotation = rotation,
                lamp = lamp,
                parentguid = parent == null ? "" : parent.guid
            };
        }

        string mainName {
            get
            {
                string i = lamp.serial;

                if (!lamp.connected)
                    i += " disconnected";

                return i;
            }
        }
    }

    [Serializable]
    public class LampItemSaveData : WorkspaceItemSaveData
    {
        public Lamp lamp;

        public override void Load()
        {
            LampManager manager = LampManager.instance;

            if (manager.Lamps.Any(_ => _.serial == lamp.serial))
                lamp = manager.Lamps.FirstOrDefault(_ => _.serial == lamp.serial);
            else
                manager.AddLamp(lamp);

            LampItemView view = lamp.AddToWorkspace(position, scale, rotation);
            view.guid = guid;
        }
    }
}
