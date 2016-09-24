using System;
using System.ComponentModel;

namespace NinjaTasks.Core.Services.Server
{
    public interface ISyncServerManager : INotifyPropertyChanged, IDisposable
    {
        DateTime LastSync { get; }
        string LastError { get; }
        bool IsRunning { get;  }
        bool IsSyncActive { get; }

        bool IsAvailable { get; }
        bool IsAvailableOnDevice { get; }
    }
}