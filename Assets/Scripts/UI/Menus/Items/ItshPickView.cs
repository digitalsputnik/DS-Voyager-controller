using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VoyagerApp.UI.Menus
{
    public class ItshPickView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] Image image = null;
        [SerializeField] Itshe itshe = Itshe.white;

        public ItshEvent onValueChanged;

        void Start()
        {
            Value = itshe;
        }

        public Itshe Value
        {
            get => itshe;
            set
            {
                image.color = value.AsColor;
                itshe = value;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            ColorwheelManager.instance.OpenColorwheel(Value, OnItshPicked);
        }

        public void OnItshPicked(Itshe itshe)
        {
            Value = itshe;
            onValueChanged?.Invoke(itshe);
        }
    }

    [Serializable]
    public class ItshEvent : UnityEvent<Itshe> { }
}