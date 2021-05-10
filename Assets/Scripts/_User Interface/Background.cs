using UnityEngine;

namespace VoyagerController.UI
{
    public class Background : MonoBehaviour
    {
        static GameObject background;
        public void Start() => background = gameObject;
        public static void Enable() => background.SetActive(true);
        public static void Disable() => background.SetActive(false);
        public static bool IsEnabled => background.activeSelf;
    }
}
