using System;
using DigitalSputnik;
using UnityEngine;
using UnityEngine.UI;

namespace VoyagerController.UI
{
    public class TextButton : MonoBehaviour
    {
        private Text _text;
        private Action _action;

        public void Awake()
        {
            _text = GetComponentInChildren<Text>();
        }

        public void Setup(string text, Action action)
        {
            _text.text = text;
            _action = action;
        }

        public void Click()
        {
            _action?.Invoke();
        }
    }
}