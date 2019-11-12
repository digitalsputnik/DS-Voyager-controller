using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using VoyagerApp.Lamps;

namespace VoyagerApp.Workspace.Views
{
    public class LampItemView : WorkspaceItemView, ISelectableItem
    {
        public static bool ShowOrder;

        public bool Selected { get; private set; }
        public Bounds Bounds => GetComponentInChildren<MeshRenderer>().bounds;
        public WorkspaceItemView View => this;

        public string prefix = "";
        public string suffix = "";

        [SerializeField] protected TextMesh nameText      = null;
        [SerializeField] protected TextMesh orderText     = null;
        [SerializeField] Color normalTextColor            = Color.black;
        [SerializeField] internal Color selectedTextColor = Color.black;

        int order = -1;
        public int Order
        {
            get => order;
            set
            {
                order = value;
                if (ShowOrder)
                    SetPrefix(order == -1 ? "" : order.ToString());
            }
        }

        public float3[] SelectPositions
        {
            get
            {
                float3[] positions = new float3[lamp.pixels];
                Vector2[] pixels = PixelWorldPositions();

                for (int i = 0; i < lamp.pixels; i++)
                    positions[i] = new float3(pixels[i], 0.0f);

                return positions;
            }
        }

        public Lamp lamp;
        bool prevCon;

        void Update()
        {
            if (prevCon != lamp.connected)
            {
                prevCon = lamp.connected;
                RedrawText();
            }
        }

        public override void Setup(object data)
        {
            lamp = (Lamp)data;
            nameText.text = lamp.serial;
            normalTextColor = nameText.color;
            base.Setup(data);
            RedrawText();
        }

        public void SetPrefix(string prefix)
        {
            this.prefix = prefix;
            RedrawText();
        }

        public void SetSuffix(string suffix)
        {
            this.suffix = suffix;
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
            List<string> info = new List<string>();
            info.Add(mainName);
            if (suffix != "") info.Add(suffix);
            nameText.text = string.Join(", ", info);

            orderText.text = prefix;
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

        public override WorkspaceItemView Load()
        {
            LampManager manager = LampManager.instance;

            if (manager.Lamps.Any(_ => _.serial == lamp.serial))
                lamp = manager.Lamps.FirstOrDefault(_ => _.serial == lamp.serial);
            else
                manager.AddLamp(lamp);

            LampItemView view = lamp.AddToWorkspace(position, scale, rotation);
            view.guid = guid;
            return view;
        }
    }
}
