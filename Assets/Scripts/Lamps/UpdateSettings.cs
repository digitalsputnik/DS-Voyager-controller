using UnityEngine;

namespace VoyagerApp.Lamps
{
    public class UpdateSettings : MonoBehaviour
    {
        static UpdateSettings instance;
        void Awake() => instance = this;

        public static string VoyagerAnimationVersion => instance.voyagerAnimVersion;

        [SerializeField] string voyagerAnimVersion;
    }
}