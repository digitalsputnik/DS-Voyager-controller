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

        public GameObject CWSliderToggle;
        public GameObject MinMaxToggle;

        public ItshEvent onValueChanged;

        void Start()
        {
            Value = itshe;
            //CWSliderToggle = GameObject.Find("CWSliderToggle");
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
            CWSliderToggle.SetActive(true);
            MinMaxToggle.GetComponent<Button>().enabled = false;
            //MinMaxToggle.SetActive(false);
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