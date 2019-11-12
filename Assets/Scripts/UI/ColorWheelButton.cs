using UnityEngine;
using UnityEngine.UI;
using VoyagerApp.Utilities;
using VoyagerApp.Workspace;

namespace VoyagerApp.UI
{
    [RequireComponent(typeof(Button))]
    public class ColorWheelButton : MonoBehaviour
    {
        [SerializeField] Image previewColor = null;

        Itshe currentItshe;
        Button button;

        void Start()
        {
            button = GetComponent<Button>();
            WorkspaceSelection.instance.onSelectionChanged += SelectionChanged;
            button.onClick.AddListener(OnClick);
            SelectionChanged();
        }

        void OnDestroy()
        {
            WorkspaceSelection.instance.onSelectionChanged -= SelectionChanged;
        }

        void SelectionChanged()
        {
            if (WorkspaceUtils.SelectedLamps.Count > 0)
            {
                currentItshe = WorkspaceUtils.SelectedLamps[0].itshe;
                previewColor.color = currentItshe.AsColor;
                previewColor.gameObject.SetActive(true);
                button.interactable = true;
            }
            else
            {
                previewColor.gameObject.SetActive(false);
                button.interactable = false;
            }
        }

        void OnClick()
        {
            ColorwheelManager.instance.OpenColorwheel(currentItshe, ItsheChanged);
        }

        void ItsheChanged(Itshe itshe)
        {
            currentItshe = itshe;
            previewColor.color = itshe.AsColor;
            foreach (var lamp in WorkspaceUtils.SelectedLamps)
                lamp.SetItshe(itshe);
        }
    }
}