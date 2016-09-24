using System;
using System.Windows.Forms;
using Cirrious.MvvmCross.ViewModels;
using InTheHand.Windows.Forms;
using NinjaTasks.App.Wpf.MvvmCross;
using NinjaTools.Connectivity.Discover;
using NinjaTools.Connectivity.ViewModels.ViewModels;
using MessageBox = System.Windows.MessageBox;

namespace NinjaTasks.App.Wpf.Views
{
    public class SelectRemoteDeviceNativeView : MvxNavigatingObject, IMvxNativeView
    {
        public object DataContext { get; set; }
        public IMvxViewModel ViewModel { get; set; }


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

                var device = new RemoteDeviceInfo(RemoteDeviceInfoType.Bluetooth, dlg.SelectedDevice.DeviceName, dlg.SelectedDevice.DeviceAddress.ToString());
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
