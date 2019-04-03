using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace elb_utilities.WinForms
{
    [DebuggerDisplay("{Value}")]
    public class NotifyValue<T> : INotifyPropertyChanged
    {
        private T _value;

        public event PropertyChangedEventHandler PropertyChanged;

        public T Value
        {
            get => _value;
            set { _value = value; NotifyPropertyChanged(); }
        }

        public NotifyValue() => _value = default;

        public NotifyValue(T value) => _value = value;

        public static implicit operator T(NotifyValue<T> notifyValue) => notifyValue.Value;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
