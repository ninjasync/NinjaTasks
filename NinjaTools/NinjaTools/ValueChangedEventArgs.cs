using System;

namespace NinjaTools
{
    public class ValueChangedEventArgs<T> : EventArgs
    {
        public T OldValue { get; set; }
        public T NewValue { get; set; }
    }
}