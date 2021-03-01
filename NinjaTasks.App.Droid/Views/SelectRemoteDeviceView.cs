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
using Android.Runtime;

#if DOT42
using Dot42.Manifest;
#endif


namespace NinjaTasks.App.Droid.Views
{
    [Register("ninjatasks.app.droid.views.SelectRemoteDeviceView")]
    [Activity(Label = "Select Device")]
    public class SelectRemoteDeviceView : BaseView
    {
        public SelectRemoteDeviceView()
        {
            
        }
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            this.SetContentView(Resource.Layout.SelectRemoteDeviceView);

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
                    global::Android.Content.PM.PackageManager.FeatureBluetooth);
            }
        }

        public bool RequestEnableBluetoothIfDisabled()
        {
            if (!IsBluetoothEnabled)
            {
                Intent enableBtIntent = new Intent(BluetoothAdapter.ActionRequestEnable);
                StartActivity(enableBtIntent);
                return false;
            }
            return true;
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.selectremotedeviceactions, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            var vm = ViewModel as SelectRemoteDeviceViewModel;
            if (vm == null) return false;

            if (item.ItemId == Resource.Id.action_refresh)
            {
                vm.Refresh();
                return true;
            }

            return false;
        }
    }
}