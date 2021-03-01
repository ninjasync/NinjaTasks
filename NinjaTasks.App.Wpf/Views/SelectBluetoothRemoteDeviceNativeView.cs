using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using InTheHand.Windows.Forms;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Platforms.Wpf.Views;
using MvvmCross.ViewModels;
using NinjaTools.Connectivity.Discover;
using NinjaTools.Connectivity.ViewModels.ViewModels;
using NinjaTools.GUI.Wpf.MvvmCrossCaliburnMicro;
using MessageBox = System.Windows.MessageBox;

namespace UnconventionalProxy.UI.Wpf.Views
{
    public class SelectBluetoothRemoteDeviceNativeView : FrameworkElement, IMvxNativeView,IMvxWpfView 
    {
        public IMvxViewModel ViewModel   { get; set; }
        public IMvxBindingContext BindingContext { get; set; }


        public void Show()
        {
            var vm = ViewModel as SelectRemoteDeviceViewModel;
            if (vm == null) return;
            try
            {
                SelectBluetoothDeviceDialog dlg = new SelectBluetoothDeviceDialog();
                //dlg.AddNewDeviceWizard = true;
                dlg.ForceAuthentication = true;

                if (dlg.ShowDialog() != DialogResult.OK)
                    return;

                var device = new Endpoint(EndpointType.Bluetooth, dlg.SelectedDevice.DeviceName,
                                          dlg.SelectedDevice.DeviceAddress.ToString()
                                          );
                vm.SelectedDevice = new RemoteDeviceViewModel(device);
                vm.Select();
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("unable to select Bluetooth Device: " + ex.Message);
            }
        }

    }
}
