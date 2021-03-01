using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InTheHand.Net.Bluetooth;
using NinjaTools.Connectivity.Connections;
using NinjaTools.Connectivity.Discover;

namespace NinjaTools.Connectivity.Bluetooth._32Feet
{
    public class BluetoothStreamSubsystem : IStreamSubsystem, IBluetoothStreamSubsystem
    {
        public string ServiceName { get { return "Bluetooth"; } }
        private readonly CancellationTokenSource _shutdown   = new CancellationTokenSource();
        
        private bool _wasRadioOn = false;
        private bool _avaiablilityLoopStarted = false;

        public bool UseBufferedStream { get; set; }
        
        public IStreamConnector GetConnector(Endpoint deviceInfo)
        {
            return new BluetoothStreamConnector(deviceInfo, UseBufferedStream);
        }

        public IStreamListener GetListener(Endpoint deviceInfo)
        {
            return new BluetoothStreamListener(Guid.Parse(deviceInfo.Port), UseBufferedStream);
        }

        public bool IsActivated
        {
            get
            {
                lock (this)
                {
                    if (!_avaiablilityLoopStarted)
                    {
                        _avaiablilityLoopStarted = true;
                        Task.Run(AvaiabilityLoop);
                    }
                }

                return CheckBluetoothRadioOn();
            }
        }

        public bool IsAvailableOnDevice => BluetoothRadio.IsSupported;

        public event PropertyChangedEventHandler PropertyChanged;

        public static bool CheckBluetoothRadioOn()
        {
            return BluetoothRadio.IsSupported &&
                   BluetoothRadio.PrimaryRadio.Mode == RadioMode.Connectable;
        }

        private async void AvaiabilityLoop()
        {
            _wasRadioOn = IsActivated;
            while (!_shutdown.IsCancellationRequested)
            {
                bool isRadioOn = IsActivated;
                try
                {
                    await Task.Delay(250, _shutdown.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    return;
                }

                if (_wasRadioOn != isRadioOn)
                {
                    _wasRadioOn = isRadioOn;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsActivated)));
                }
            }
        }

        public void Dispose()
        {
            _shutdown.Cancel();
        }

    }
}
