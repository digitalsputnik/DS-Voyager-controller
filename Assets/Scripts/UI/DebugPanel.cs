using UnityEngine;
using UnityEngine.UI;
using VoyagerApp.Utilities;

namespace VoyagerApp.UI
{
    public class DebugPanel : MonoBehaviour
    {
        [SerializeField] GameObject panelObject = null;
        [SerializeField] Text timesyncText = null;

        void Start()
        {
            if (!ApplicationState.DeveloperMode)
            {
                gameObject.SetActive(false);
                return;
            }
        }


        void Update()
        {
            if (panelObject.activeSelf)
            {
                timesyncText.text = $"TIMESYNC: {NetUtils.VoyagerClient.TimeOffset}";
            }
        }
    }
}