using UnityEngine;
using UnityEngine.UI;
using VoyagerApp.Workspace;
using VoyagerApp.Workspace.Views;

namespace VoyagerApp.UI.Menus
{
    public class CreateGroupMenu : Menu
    {
        [SerializeField] InputField nameField;
        [SerializeField] Button createButton;

        internal override void OnShow()
        {
            nameField.onValueChanged.AddListener(NameFieldChanged);
            nameField.text = "group";
        }

        internal override void OnHide()
        {
            nameField.onValueChanged.RemoveListener(NameFieldChanged);
        }

        void NameFieldChanged(string text)
        {
            createButton.interactable = text != "";
        }

        public void Create()
        {
            WorkspaceManager.instance.InstantiateItem<GroupItemView>(nameField.text);
            GetComponentInParent<InspectorMenuContainer>().ShowMenu(null);
        }
    }
}