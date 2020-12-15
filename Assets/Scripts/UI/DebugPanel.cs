using UnityEngine;
using UnityEngine.UI;
using VoyagerApp.Utilities;

namespace VoyagerApp.UI
{
    public class DebugPanel : MonoBehaviour
    {
        [SerializeField] private GameObject _panelObject = null;
        [SerializeField] private Text _text = null;

        private void Start()
        {
            if (ApplicationState.DeveloperMode) return;
            gameObject.SetActive(false);
        }


        private void Update()
        {
            if (_panelObject.activeSelf)
            {
                _text.text = $"TIME OFFSET: {NetUtils.VoyagerClient.TimeOffset}\n" + 
                             $"SYSTEM TIME: {TimeUtils.Epoch}\n" +
                             $"LAMP TIME: {TimeUtils.Epoch + NetUtils.VoyagerClient.TimeOffset}";
            }
        }
    }
}