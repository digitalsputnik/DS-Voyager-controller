using UnityEngine;

namespace VoyagerController.UI
{
    public class Tutorial : MonoBehaviour
    {
        public int Order;

        public string title;
        [TextArea(3, 10)]
        public string info;
        [TextArea(3, 10)]
        public string moreInfo;

        public bool leftMenu;
        public bool rightMenu;
        public bool disableRightButton;
        public bool disableLeftButton;

        public bool clearWorkspace;
        public bool clearLampSelection;

        public int backJump;

        public Menu rightMenuToOpen = null;
        public Menu leftMenuToOpen = null;

        public enum CenterOverlay
        {
            None,
            Top,
            Default,
            MoreInfo,
            Prompt
        };

        [SerializeField] public CenterOverlay centerOverlay = CenterOverlay.Default;

        public virtual void CheckForAction() { }
    }
}
