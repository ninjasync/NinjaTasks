using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Java.IO;
using Java.Util;
using NinjaTools.Connectivity;
using NinjaTools.Connectivity.Connections;
using NinjaTools.Connectivity.Discover;
using NinjaTools.Connectivity.Streams;
using NinjaTools.Droid;
using NinjaTools.Logging;
using OperationCanceledException = System.OperationCanceledException;

namespace NinjaTasks.App.Droid.Services
{
    public class AndroidBluetoothStreamSubsystem : IStreamSubsystem, IBroadcastReceiver, IDisposable, IBluetoothStreamSubsystem
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private readonly Context _ctx;
        private readonly BluetoothAdapter _bluetoothAdapter;
        private BroadcastListener _updateReceiver;
        public event PropertyChangedEventHandler PropertyChanged;

        public bool UseBufferedStream { get; set; } = true;

        public AndroidBluetoothStreamSubsystem(Context ctx)
        {
            _ctx = ctx ?? Application.Context;
            _bluetoothAdapter = BluetoothAdapter.DefaultAdapter;

            if (_bluetoothAdapter != null)
                _ctx.RegisterReceiver(_updateReceiver = new BroadcastListener(this),
                                          new IntentFilter(BluetoothAdapter.ActionStateChanged));

        }

        public string ServiceName { get { return "Bluetooth"; } }

        public IStreamConnector GetConnector(Endpoint endpoint)
        {
            var device = _bluetoothAdapter.BondedDevices.FirstOrDefault(d => d.Address == endpoint.Address);
            if (device == null)
                throw new Exception("device no longer bonded.");

            return new AndroidBluetoothConnector(device, this, endpoint.Port);
        }

        public IStreamListener GetListener(Endpoint deviceInfo)
        {
            return new AndroidBluetoothListener(this, deviceInfo.Name, deviceInfo.Port);
        }

        public bool IsActivated { get { return _bluetoothAdapter != null && _bluetoothAdapter.IsEnabled; } }
        public bool IsAvailableOnDevice { get { return _bluetoothAdapter != null; } }

        public void OnBroadcastReceived(Context context, Intent intent)
        {
            if (intent.Action != BluetoothAdapter.ActionStateChanged)
                return;

            OnPropertyChanged(new PropertyChangedEventArgs("IsActivated"));
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, e);
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
            private readonly AndroidBluetoothStreamSubsystem _parent;
            private readonly string _serviceId;

            public AndroidBluetoothConnector(BluetoothDevice device, AndroidBluetoothStreamSubsystem parent, string serviceId)
            {
                _device = device;
                _parent = parent;
                _serviceId = serviceId;
            }

            public async Task<Stream> ConnectAsync(CancellationToken cancel)
            {
                BluetoothSocket socket = null;
                try
                {
                    var uuid = UUID.FromString(_serviceId);
                    Log.Debug($"about to connect to {_device.Address} at service uuid: {uuid}");

                    //bool success = false;
                    try
                    {
                        socket = _device.CreateRfcommSocketToServiceRecord(uuid);
                        using (cancel.Register(socket.Close))
                            await Task.Run(async () => await socket.ConnectAsync(), cancel);
                        //success = true;
                    }
                    catch (Exception ex) when (!(ex is OperationCanceledException))
                    {
                        cancel.ThrowIfCancellationRequested();

                        ParcelUuid[] uuids = _device.GetUuids();


                        Log.Info($"failed to connect to {_device.Address} at service uuid: {uuid}: " + ex.ToString());
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

                    Stream inputStream = socket.InputStream;
                    Stream outputStream = socket.OutputStream;

                    if (_parent.UseBufferedStream)
                    {
                        inputStream = new BufferedStream(inputStream);
                        outputStream = new BufferedStream(outputStream);
                    }

                    Stream stream = new CombinedStream(inputStream, outputStream, new CloseOnDispose(socket));
                    return new TimeoutStream(stream);
                }
                catch (Exception)
                {
                    if (socket != null)
                    {
                        socket.Close();
                        socket.Dispose();
                    }
                    throw;
                }
            }

            public bool IsAvailable { get { return _parent.IsActivated; } }
            public bool WasCancelled { get; private set; }
            public void Dispose()
            {
            }
        }

        private class AndroidBluetoothListener : IStreamListener
        {
            private readonly AndroidBluetoothStreamSubsystem _parent;
            private readonly string _serviceName;
            private readonly string _serviceId;
            private BluetoothServerSocket _serverSocket;

            public AndroidBluetoothListener(AndroidBluetoothStreamSubsystem parent, string serviceName, string serviceId)
            {
                _parent = parent;
                _serviceName = serviceName;
                _serviceId = serviceId;
            }

            public async Task<Stream> ListenAsync(CancellationToken cancel)
            {
                BluetoothSocket socket = null;
                try
                {
                    if (_serverSocket == null)
                    {
                        UUID uuid = UUID.FromString(_serviceId);
                        Log.Debug($"about to list at service uuid: {uuid}");
                        _serverSocket = BluetoothAdapter.DefaultAdapter.ListenUsingRfcommWithServiceRecord(_serviceName, uuid);
                    }

                    using (cancel.Register(_serverSocket.Close))
                        socket = await Task.Run(async () => await _serverSocket.AcceptAsync(), cancel);

                    Log.Debug($"got at socket while listening for service uuid: {_serviceId}");

                    Stream inputStream = socket.InputStream;
                    Stream outputStream = socket.OutputStream;

                    if (_parent.UseBufferedStream)
                    {
                        inputStream = new BufferedStream(inputStream);
                        outputStream = new BufferedStream(outputStream);
                    }

                    Stream stream = new CombinedStream(inputStream, outputStream, new CloseOnDispose(socket));

                    return new TimeoutStream(stream);
                }
                catch (Exception ex)
                {
                    CloseServerSocket();
                    // create a new one on next attempt.
                    _serverSocket = null;

                    if (!cancel.IsCancellationRequested)
                    {
                        Log.Info($"error while listening for uuid {_serviceId}" + ex.ToString());
                        throw;

                    }
                    else
                    {
                        Log.Debug($"stopped listening for service uuid: {_serviceId}");
                        cancel.ThrowIfCancellationRequested();
                        return null;
                    }
                }
            }

            public bool IsAvailable => _parent.IsActivated;

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
                        _serverSocket.Dispose();
                    }
                    catch (Exception)
                    {
                    }
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
                catch (Exception) { }

                _closable.Dispose();
            }
        }


    }
}
