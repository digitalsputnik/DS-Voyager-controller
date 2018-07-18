using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Voyager.Lamps;

namespace Voyager.Lamps
{
	public class PhysicalLamp : MonoBehaviour {
		
		public Lamp Owner;
		[Space(5)]
		public LampType Type;
        [Space(2)]
        public TextMesh Text;
        public GameObject DisconnectionSprite;
		[Space(2)]
		public bool MovingInWorkspace;

        public LampMove move;

        public void Setup(Lamp owner)
		{
			Owner = owner;
            InvokeRepeating("UpdateUI", 0.0f, 1.0f);
			move = GetComponent<LampMove>();
        }

        void UpdateUI()
        {
			Text.text = Owner.Name + " " + Owner.Serial + " " + Owner.BatteryLevel;
			Text.text += Owner.ConnectionLost ? " Disconnected" : " Connected";
			DisconnectionSprite.SetActive(Owner.ConnectionLost);
		}

		void Update()
		{
			MovingInWorkspace = move.moving;
		}
	}
}