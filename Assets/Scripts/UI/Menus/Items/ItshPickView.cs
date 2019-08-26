using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VoyagerApp.UI.Menus
{
    public class ItshPickView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] Image image    = null;
        [SerializeField] Itsh itsh      = Itsh.white;

        public ItshEvent onValueChanged;

        void Start()
        {
            Value = itsh;
        }

        public Itsh Value
        {
            get => itsh;
            set
            {
                image.color = value.AsColor;
                itsh = value;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            ColorwheelManager.instance.OpenColorwheel(Value, (itsh) =>
            {
                Value = itsh;
                onValueChanged?.Invoke(itsh);
            });
        }
    }

    [Serializable]
    public class ItshEvent : UnityEvent<Itsh> { }
}