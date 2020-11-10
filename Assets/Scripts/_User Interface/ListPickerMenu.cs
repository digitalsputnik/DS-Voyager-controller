using System;
using UnityEngine.EventSystems;

namespace VoyagerController.UI
{
    public class ListPickerMenu : Menu, IPointerDownHandler, IPointerUpHandler
    {
        private static ListPickerMenu _instance;
        private void Awake() => _instance = this;

        public static void PickValue(string title, int index, string[] values, Action<int> picked)
        {
            
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            
        }
    }
}