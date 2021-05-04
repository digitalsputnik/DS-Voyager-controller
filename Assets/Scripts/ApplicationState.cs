using UnityEngine;

namespace VoyagerController
{
    public static class ApplicationState
    {
        public static readonly EventValue<SelectionMode> SelectMode = new EventValue<SelectionMode>(SelectionMode.Set);
        public static readonly EventValue<ControllingMode> ControlMode = new EventValue<ControllingMode>(ControllingMode.Items);
        public static readonly EventValue<bool> Identifying = new EventValue<bool>(false);
        public static readonly EventValue<bool> ColorWheelActive = new EventValue<bool>(false);
        public static readonly EventValue<GlobalPlaymode> Playmode = new EventValue<GlobalPlaymode>(GlobalPlaymode.Play);
        public static readonly EventValue<double> PlaymodePausedSince = new EventValue<double>(0.0);
        public static readonly EventValue<float> GlobalDimmer = new EventValue<float>(1.0f);

        public static event StateEventHandler OnNewProject;
        public static void RaiseNewProject() => OnNewProject?.Invoke();

        public static bool DeveloperMode => Debug.isDebugBuild;
    }

    public enum SelectionMode { Set, Add, Remove }
    public enum ControllingMode { Items, Camera, CameraToggled }
    public enum GlobalPlaymode { Play, Pause, Stop }

    public delegate void StateEventHandler();
}