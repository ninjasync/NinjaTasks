using System;
using NinjaTools;
using NinjaTools.Connectivity.Discover;

namespace NinjaTasks.App.Droid.Services
{
    public class GuardBasedScanContext : IScanContext
    {
        private readonly IDiscoverRemoteEndpoints _parent;
        private readonly Guard _guard;
        private IDisposable _token;

        public GuardBasedScanContext(IDiscoverRemoteEndpoints parent, Guard guard)
        {
            _parent = parent;
            _guard = guard;
            RestartScanning();
        }

        public bool IsScanning { get { return _parent.IsScanning; } }
        public void StopScanning()
        {
            if (_token == null) return;
            _token.Dispose();
            _token = null;
        }

        public void RestartScanning()
        {
            if (_token != null) return;
            _token = _guard.Use();
        }
        public void Dispose()
        {
            StopScanning();
        }
    }
}