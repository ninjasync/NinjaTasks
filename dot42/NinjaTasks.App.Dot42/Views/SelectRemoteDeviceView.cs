// --------------------------------------------------------------------------------------------------------------------
// <summary>
//    Defines the Rennrad type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Views;
using NinjaTools.Connectivity.ViewModels.ViewModels;
using NinjaTools.Droid.MvvmCross;

#if DOT42
using Dot42.Manifest;
#endif


namespace NinjaTasks.App.Droid.Views
{
    [Activity(Label = "Select Device", VisibleInLauncher = false)]
    public class SelectRemoteDeviceView : BaseView
    {
        public SelectRemoteDeviceView()
        {
            
        }
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            this.SetContentView(R.Layout.SelectRemoteDeviceView);

            RequestEnableBluetoothIfDisabled();
        }

        public bool IsBluetoothEnabled
        {
            get
            {
                try
                {
                    return BluetoothAdapter.DefaultAdapter != null && BluetoothAdapter.DefaultAdapter.IsEnabled;
                }
                catch (Exception)
                {
                    return false;
                }
                
            }
        }

        public bool IsBluetoothSupportedOnDevice
        {
            get
            {
                return PackageManager.HasSystemFeature(
                    global::Android.Content.PM.PackageManager.FEATURE_BLUETOOTH);
            }
        }

        public bool RequestEnableBluetoothIfDisabled()
        {
            if (!IsBluetoothEnabled)
            {
                Intent enableBtIntent = new Intent(BluetoothAdapter.ACTION_REQUEST_ENABLE);
                StartActivity(enableBtIntent);
                return false;
            }
            return true;
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(R.Menu.SelectRemoteDeviceActions, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            var vm = ViewModel as SelectRemoteDeviceViewModel;
            if (vm == null) return false;

            if (item.ItemId == R.Id.action_refresh)
            {
                vm.Refresh();
                return true;
            }

            return false;
        }
    }
}