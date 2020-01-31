using UnityEngine;
using UnityEngine.UI;

namespace VoyagerApp.UI
{
    public class InputFieldMenu : Menu
    {
        static InputFieldMenu instance;

        [SerializeField] Text _titleText = default;
        [SerializeField] InputField _inputField = default;
        [SerializeField] Button _doneButton = default;
        [SerializeField] Button _cancelButton = default;

        InputFieldMenuDoneHandler _onDone;
        InputFieldMenuCancelHandler _onCancel;
        int _minCharacters;

        void Awake()
        {
            instance = this;

            _inputField.onValueChanged.AddListener(OnFieldEdited);
            _cancelButton.onClick.AddListener(CancelClicked);
            _doneButton.onClick.AddListener(DoneClicked);
        }

        public static void Show(string title, string text, InputFieldMenuDoneHandler onDone, int minCharacters = 0, bool allowCancel = true, InputFieldMenuCancelHandler onCancel = null)
        {
            instance.InstanceShow(title, text, minCharacters, onDone, allowCancel, onCancel);
        }

        void InstanceShow(string title, string text, int minCharacters, InputFieldMenuDoneHandler onDone, bool allowCancel, InputFieldMenuCancelHandler onCancel)
        {
            _titleText.text = title;
            _inputField.text = text;

            _onDone = onDone;
            _onCancel = onCancel;
            _minCharacters = minCharacters;

            _cancelButton.gameObject.SetActive(allowCancel);

            Open = true;
        }

        void OnFieldEdited(string text)
        {
            _doneButton.interactable = text.Length >= _minCharacters;
        }

        void DoneClicked()
        {
            _onDone?.Invoke(_inputField.text);
            Open = false;
        }

        void CancelClicked()
        {
            _onCancel?.Invoke();
            Open = false;
        }
    }

    public delegate void InputFieldMenuDoneHandler(string text);
    public delegate void InputFieldMenuCancelHandler();
}