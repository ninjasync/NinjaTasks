using System;

namespace NinjaTools.Connectivity.Discover
{
    public interface IScanContext : IDisposable
    {
        bool IsScanning { get; }
        void StopScanning();
        
        void RestartScanning();
    }
}