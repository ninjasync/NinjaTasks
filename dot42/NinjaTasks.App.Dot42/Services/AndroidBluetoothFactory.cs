using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Java.IO;
using Java.Util;
using NinjaTasks.Core.Services;
using NinjaTools.Connectivity.Connections;
using NinjaTools.Connectivity.Discover;
using NinjaTools.Connectivity.Streams;
using NinjaTools.Droid;
using NinjaTools.Logging;

namespace NinjaTasks.App.Droid.Services
{
    public class AndroidBluetoothFactory : IBluetoothStreamFactory, IBroadcastReceiver, IDisposable
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private readonly Context _ctx;
        private readonly BluetoothAdapter _bluetoothAdapter;
        private BroadcastListener _updateReceiver;
        public event PropertyChangedEventHandler PropertyChanged;

        public AndroidBluetoothFactory(Context ctx)
        {
            _ctx = ctx;
            _bluetoothAdapter = BluetoothAdapter.DefaultAdapter;

            if (_bluetoothAdapter != null)
                _ctx.RegisterReceiver(_updateReceiver = new BroadcastListener(this),
                                          new IntentFilter(BluetoothAdapter.ACTION_STATE_CHANGED));                

        }

        public string ServiceName { get { return "Bluetooth"; }}

        public IStreamConnector GetConnector(RemoteDeviceInfo endpoint)
        {
            var device = _bluetoothAdapter.BondedDevices.FirstOrDefault(d=>d.Address == endpoint.Address);
            if(device == null)
                throw new Exception("device no longer bonded.");

            return new AndroidBluetoothConnector(device, this, endpoint.Port);
        }

        public IStreamListener GetListener(RemoteDeviceInfo deviceInfo)
        {
            return new AndroidBluetoothListener(deviceInfo.Name, deviceInfo.Port);
        }

        public bool IsActivated { get { return _bluetoothAdapter != null && _bluetoothAdapter.IsEnabled; } }
        public bool IsAvailableOnDevice { get { return _bluetoothAdapter != null; } }

        public void OnBroadcastReceived(Context context, Intent intent)
        {
            if (intent.Action != BluetoothAdapter.ACTION_STATE_CHANGED)
                return;

            OnPropertyChanged(new PropertyChangedEventArgs("IsActivated"));
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, e);
        }


        public void Dispose()
        {
            if (_updateReceiver != null)
                _ctx.UnregisterReceiver(_updateReceiver);
            _updateReceiver = null;
        }

        private class AndroidBluetoothConnector : IStreamConnector
        {
            private readonly BluetoothDevice _device;
            private readonly AndroidBluetoothFactory _parent;
            private readonly string _serviceId;

            public AndroidBluetoothConnector(BluetoothDevice device, AndroidBluetoothFactory parent, string serviceId)
            {
                _device = device;
                _parent = parent;
                _serviceId = serviceId;
            }

            public async Task<Stream> ConnectAsync()
            {
                BluetoothSocket socket = null;
                try
                {
                    var uuid = UUID.FromString(_serviceId);
                    Log.Debug("service uuid: {0}", uuid);

                    bool success = false;
                    try
                    {
                        socket = _device.CreateRfcommSocketToServiceRecord(uuid);
                        await Task.Run(()=>socket.Connect());
                        success = true;
                    }
                    catch (Exception)
                    {
                        ParcelUuid[] uuids = _device.Uuids;

                        foreach (var uu in uuids)
                            Log.Debug("bluetooth supported service: uuid: {0}", uu.Uuid);

                        throw;
                    }

                    //if (!success)
                    //{

                    //    //_device.FetchUuidsWithSdp();
                    //    //throw new SyncTryLaterException(TimeSpan.FromSeconds(30));

                    //    //_device.

                    //    //int idx = -1;
                    //    //for(int i = 0; i < uuids.Length; ++i)
                    //    //    if (uuids[i].Equals(uuid))
                    //    //    {
                    //    //        idx = i;
                    //    //        break;
                    //    //    }

                    //    // try again with this workaround...
                    //    //// http://stackoverflow.com/questions/3031796/disconnect-a-bluetooth-socket-in-android
                    //    //// and http://t7929.gnome-mono-monodroid.monotalk.info/createrfcommsockettoservicerecord-workaround-t7929.html
                    //    //IntPtr createRfcommSocket = JNIEnv.GetMethodID(_device.Class.Handle, "createRfcommSocket",
                    //    //                                               "(I)Landroid/bluetooth/BluetoothSocket;");
                    //    //IntPtr socketHandle = JNIEnv.CallObjectMethod(_device.Handle, createRfcommSocket, new JValue(1));
                    //    //// `socketHandle` now holds a BluetoothSocket instance; do something with it...                        
                    //    //socket = Object.GetObject<BluetoothSocket>(socketHandle, JniHandleOwnership.TransferLocalRef);
                    //    //await socket.ConnectAsync();

                    //}
                    

                    var combinedStream = new CombinedStream(new JavaInputStreamWrapper(socket.InputStream), new JavaOutputStreamWrapper(socket.OutputStream), new CloseOnDispose(socket));
                    return combinedStream.EnsureTimeoutCapable();
                }
                catch (Exception)
                {
                    if (socket != null)
                    {
                        socket.Close();
                    }
                    throw;
                }
            }

            public bool IsAvailable { get { return _parent.IsActivated; } }
            public bool WasCancelled { get; private set; }
        }

        private class AndroidBluetoothListener : IStreamListener
        {
            private readonly string _serviceName;
            private readonly string _serviceId;
            private readonly Context _context;
            private BluetoothServerSocket _serverSocket;

            public AndroidBluetoothListener(string serviceName, string serviceId)
            {
                _serviceName = serviceName;
                _serviceId = serviceId;
            }

            public async Task<Stream> ListenAsync(CancellationToken cancel)
            {
                BluetoothSocket socket = null;
                try
                {
                    if(_serverSocket == null)
                    {
                        UUID uuid = UUID.FromString(_serviceId);
                        _serverSocket = BluetoothAdapter.DefaultAdapter
                            .ListenUsingRfcommWithServiceRecord(_serviceName, uuid);
                    }

                    socket = await Task.Run(() => _serverSocket.Accept(), cancel);
                    var combinedStream = new CombinedStream(new JavaInputStreamWrapper(socket.InputStream), new JavaOutputStreamWrapper(socket.OutputStream), new CloseOnDispose(socket));
                    return new TimeoutStream(combinedStream);
                }
                catch (Exception)
                {
                    CloseServerSocket();
                    // create a new one on next attempt.
                    _serverSocket = null;
                    throw;
                }
            }

            public void Dispose()
            {
                CloseServerSocket();
            }

            private void CloseServerSocket()
            {
                if (_serverSocket != null)
                {
                    try
                    {
                        _serverSocket.Close();
                    }
                    catch (Exception){}
                }
            }
        }

        private class CloseOnDispose : IDisposable
        {
            private readonly ICloseable _closable;

            public CloseOnDispose(ICloseable closable)
            {
                _closable = closable;
            }

            public void Dispose()
            {
                try
                {
                    _closable.Close();
                }
                catch (Exception){}
            }
        }


    }
}
