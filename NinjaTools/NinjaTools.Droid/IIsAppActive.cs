using System.ComponentModel;

namespace NinjaTools.Droid
{
    public interface IIsAppActive : INotifyPropertyChanged
    {
        bool IsInForeground { get; }
    }
}