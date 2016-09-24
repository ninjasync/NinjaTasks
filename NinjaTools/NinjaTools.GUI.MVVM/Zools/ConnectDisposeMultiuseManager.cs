using System;
using System.Collections.Generic;

namespace NinjaTools.MVVM.Zools
{
    public class ConnectDisposeMultiuseManager : IDisposable
    {
        private static readonly Dictionary<IDisposable, Guard> DisposeGuards = new Dictionary<IDisposable, Guard>();
        private static readonly Dictionary<IDisposable, Guard> ConnectGuards = new Dictionary<IDisposable, Guard>();
        private static readonly object Sync = new object();

        private readonly IDisposable _obj;
        private readonly Action _connect;
        private readonly Action _disconnect;
        private GuardToken _connectToken;
        private GuardToken _disposeToken;

        public ConnectDisposeMultiuseManager(IDisposable obj, Action connect, Action disconnect)
        {
            _obj = obj;
            _connect = connect;
            _connect = connect;
            _disconnect = disconnect;

            Guard guard;
            lock (Sync)
            {
                if (!DisposeGuards.TryGetValue(obj, out guard))
                    DisposeGuards.Add(obj, guard = new Guard());
            }
            _disposeToken = guard.Use();
        }

        public void Connect()
        {
            if (_connectToken == null)
            {
                Guard guard;
                lock (Sync)
                {
                    if (!ConnectGuards.TryGetValue(_obj, out guard))
                        ConnectGuards.Add(_obj, guard = new Guard());
                }
                _connectToken = guard.Use();
            }

            _connect();
        }

        public void Disconnect()
        {
            bool shouldDisconnect = false;
            if (_connectToken != null)
            {
                shouldDisconnect = _connectToken.Done();
                _connectToken = null;
            }

            if (shouldDisconnect)
                _disconnect();
        }

        public void Dispose()
        {
            if (_disposeToken == null) return;
            IDisposable dispose = null;

            lock (Sync)
                if (_disposeToken.Done())
                {
                    DisposeGuards.Remove(_obj);
                    ConnectGuards.Remove(_obj);
                    dispose = _obj;
                }

            if(dispose != null)
                dispose.Dispose();

            _disposeToken = null;
        }
    }
}
