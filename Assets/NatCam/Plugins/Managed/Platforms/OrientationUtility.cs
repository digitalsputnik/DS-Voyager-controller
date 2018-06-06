/* 
*   NatCam
*   Copyright (c) 2018 Yusuf Olokoba
*/

namespace NatCamU.Core.Platforms {

    using UnityEngine;
    using System;

    public sealed class OrientationUtility : MonoBehaviour {

        #region --Properties--

        public static int Orientation {
            get {
                if (!Application.isMobilePlatform) return 0;
                switch (instance.lastOrientation) {
                    case DeviceOrientation.LandscapeLeft: return 0;
                    case DeviceOrientation.Portrait: return 1;
                    case DeviceOrientation.LandscapeRight: return 2;
                    default: return 1; // Why not 0?
                }
            }
        }
        #endregion


        #region --Op vars--
        public static event Action onOrient;
        private DeviceOrientation lastOrientation = 0;
        private static readonly OrientationUtility instance;
        #endregion


        #region --Operations--

        static OrientationUtility () {
            instance = new GameObject("NatCam Orientation Utility").AddComponent<OrientationUtility>();
        }

        void Awake () {
            DontDestroyOnLoad(this.gameObject);
            DontDestroyOnLoad(this);
            CheckOrientation();
        }

        void Update () {
            CheckOrientation();
        }

        void CheckOrientation () {
            var reference = (DeviceOrientation)(int)Screen.orientation; // Input.deviceOrientation
            switch (reference) {
                case DeviceOrientation.FaceDown:
                case DeviceOrientation.FaceUp:
                case DeviceOrientation.Unknown: break;
                default:
                    if (lastOrientation != reference) {
                        lastOrientation = reference;
                        if (onOrient != null) onOrient();
                    }
                break;
            }
        }
        #endregion
    }
}