using UnityEngine;
using UnityEngine.UI;

namespace VoyagerController.UI
{
    [RequireComponent(typeof(CanvasScaler))]
    public class UIScaleDeviceBased : MonoBehaviour
    {
        private void Awake()
        {
            if (GetScreenSize() < 7)
                GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
            else if (GetScreenSize() >= 7)
                GetComponent<CanvasScaler>().referenceResolution = new Vector2(2560, 1600);
        }

        private static float GetScreenSize()
        {
            return Mathf.Sqrt(Screen.width * Screen.width + Screen.height * Screen.height) / Screen.dpi;
        }
    }   
}