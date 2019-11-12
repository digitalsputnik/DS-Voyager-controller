namespace VoyagerApp.UI
{
    public static class ApplicationState
    {
        public static EventValue<SelectionState> SelectionMode = new EventValue<SelectionState>(UI.SelectionState.Set);
        public static EventValue<bool> Identifying = new EventValue<bool>(false);
        public static EventValue<bool> ColorWheelActive = new EventValue<bool>(false);
    }

    public enum SelectionState { Set, Add, Remove }
}