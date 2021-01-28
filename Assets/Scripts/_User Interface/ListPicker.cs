using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace VoyagerController.UI
{
    [RequireComponent(typeof(Button))]
    public class ListPicker : MonoBehaviour
    {
        [SerializeField] private string _title = "";
        [SerializeField] private int _index = 0;
        [SerializeField] private string[] _values = null;
        [SerializeField] private UnityEvent _opened = null;
        [SerializeField] private UnityEvent _changed = null;
        
        public int Index
        {
            get => _index;
            set => _index = value;
        }

        public UnityEvent Opened => _opened;
        public UnityEvent Changed => _changed;
        public string Selected => _values.Length == 0 ? string.Empty : _values[_index];

        private void Start()
        {
            GetComponent<Button>().onClick.AddListener(ChooseItem);
        }

        public void SetItems(params string[] values)
        {
            _values = values;
            OnIndexChanged(Mathf.Clamp(_index, 0, _values.Length - 1));
        }

        public bool Interactable
        {
            get => GetComponent<Button>().interactable;
            set => GetComponent<Button>().interactable = value;
        }

        private void ChooseItem()
        {
            _opened?.Invoke();
            ListPickerMenu.instance.PickValue(_title, _index, _values, OnIndexChanged);
        }

        private void OnIndexChanged(int index)
        {
            _index = index;
            GetComponentInChildren<Text>().text = Selected;
            _changed?.Invoke();
        }
    }
}