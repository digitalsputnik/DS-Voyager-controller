namespace VoyagerController
{
    public class EventValue<T>
    {
        public event ValueChangeHandler<T> OnChanged;

        private T _value;

        public EventValue(T value)
        {
            _value = value;
        }

        public T Value
        {
            get => _value;
            set
            {
                _value = value;
                OnChanged?.Invoke(_value);
            }
        }
    }

    public delegate void ValueChangeHandler<T>(T value);
}