using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VoyagerApp.Networking.Voyager;
using VoyagerApp.Utilities;
using VoyagerApp.Workspace;

namespace VoyagerApp.UI
{
    public class IdentifySelectedLamps : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] Color pressedColor = Color.white;
        [SerializeField] Color releasedColor = Color.white;
        [SerializeField] Image image = null;

        bool send;
        float timestamp;

        void Start()
        {
            WorkspaceSelection.instance.onSelectionChanged += OnSelectionChanged;
            OnPointerUp(null);
            gameObject.SetActive(false);
        }

        void Update()
        {
            if (send && Time.time - timestamp > 0.16f)
            {
                Itshe itshe;
                if (PlayerPrefs.HasKey("identify_itsh"))
                {
                    string json = PlayerPrefs.GetString("identify_itsh");
                    itshe = JsonConvert.DeserializeObject<Itshe>(json);
                }
                else
                    itshe = new Itshe(Color.red, 1.0f);

                var packet = new PixelOverridePacket(itshe, 0.3f);
                foreach (var lamp in WorkspaceUtils.SelectedLamps)
                    NetUtils.VoyagerClient.SendPacket(lamp, packet, VoyagerClient.PORT_SETTINGS);
                timestamp = Time.time;
            }
        }

        void OnDestroy()
        {
            WorkspaceSelection.instance.onSelectionChanged -= OnSelectionChanged;
        }

        void OnSelectionChanged()
        {
            gameObject.SetActive(WorkspaceUtils.SelectedLamps.Count != 0);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            image.color = pressedColor;
            send = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            image.color = releasedColor;
            send = false;
        }
    }
}