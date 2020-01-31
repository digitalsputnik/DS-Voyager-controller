using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace VoyagerApp.UI
{
    [RequireComponent(typeof(Button))]
    public class ListPicker : MonoBehaviour
    {
        public string title;
        public int index;
        public List<string> items = new List<string>();

        public UnityEvent onOpen;
        public UnityEvent onChanged;

        public string selected
        {
            get
            {
                if (items.Count == 0)
                    return string.Empty;
                return items[index];
            }
        }

        public bool interactable
        {
            get => GetComponent<Button>().interactable;
            set => GetComponent<Button>().interactable = value;
        }

        void Start()
        {
            GetComponent<Button>().onClick.AddListener(ChooseItem);
        }

        public void SetItems(params string[] items)
        {
            this.items = items.ToList();
            int i = Mathf.Clamp(index, 0, items.Length - 1);
            OnIndexChanged(i);
        }

        void ChooseItem()
        {
            onOpen?.Invoke();
            ListPickerMenu.instance.PickValue(
                title,
                index,
                items.ToArray(),
                OnIndexChanged
            );
        }

        void OnIndexChanged(int value)
        {
            index = value;
            GetComponentInChildren<Text>().text = selected;
            onChanged?.Invoke();
        }
    }
}