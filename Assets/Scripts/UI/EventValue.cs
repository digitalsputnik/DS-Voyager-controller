using System.Collections.Generic;

namespace VoyagerApp
{
    public class EventValue<T>
    {
        public event ValueChangeHandler<T> onChanged;

        T _value;

        public EventValue(T value)
        {
            _value = value;
        }

        public T value
        {
            get => _value;
            set
            {
                //if (!EqualityComparer<T>.Default.Equals(_value, value))
                //{
                //    _value = value;
                //    onChanged?.Invoke(_value);
                //}

                _value = value;
                onChanged?.Invoke(_value);
            }
        }
    }

    public delegate void ValueChangeHandler<T>(T value);
}