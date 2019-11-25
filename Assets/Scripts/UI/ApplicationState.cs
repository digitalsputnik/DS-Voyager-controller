namespace VoyagerApp.UI
{
    public static class ApplicationState
    {
        public static EventValue<SelectionMode> SelectionMode = new EventValue<SelectionMode>(UI.SelectionMode.Set);
        public static EventValue<ControllingMode> ControllingMode = new EventValue<ControllingMode>(UI.ControllingMode.Items);
        public static EventValue<bool> Identifying = new EventValue<bool>(false);
        public static EventValue<bool> ColorWheelActive = new EventValue<bool>(false);
        public static EventValue<GlobalPlaymode> Playmode = new EventValue<GlobalPlaymode>(GlobalPlaymode.Play);
        public static EventValue<double> PlaymodePausedSince = new EventValue<double>(0.0);
        public static EventValue<float> GlobalDimmer = new EventValue<float>(1.0f);
    }

    public enum SelectionMode { Set, Add, Remove }
    public enum ControllingMode { Items, Camera, CameraToggled }
    public enum GlobalPlaymode { Play, Pause, Stop }
}