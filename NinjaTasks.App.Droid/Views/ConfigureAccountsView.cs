using Android.App;
using Android.OS;
using NinjaTools.Droid.MvvmCross;
using System;
using Android;
using Android.Bluetooth;
using Android.Content;
using Android.Content.PM;
using Android.Provider;
using Android.Views;
using Android.Widget;
using NinjaTools.Connectivity;
using MvvmCross;

namespace NinjaTasks.App.Droid.Views
{
    [Activity(Label = "@string/app_name", Icon="@drawable/ic_launcher", Exported = true)]
    public class ConfigureAccountsView : BaseView
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.ConfigureAccounts);

            FindViewById<Button>(Resource.Id.enableBluetooth).Click += OnActivateBluetooth;

            if (!IsBatteryOptimizationDisabled)
            {
                FindViewById<Button>(Resource.Id.allowToRunInBackground).Click += OnAllowInBackground;
            }
            else
            {
                FindViewById<LinearLayout>(Resource.Id.allowToRunInBackgroundContainer).Visibility = ViewStates.Gone;
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (requestCode == 100 && IsBatteryOptimizationDisabled)
            {
                FindViewById<LinearLayout>(Resource.Id.allowToRunInBackgroundContainer).Visibility = ViewStates.Gone;
            }
        }

        private void OnAllowInBackground(object sender, EventArgs e)
        {
            if (!IsBatteryOptimizationDisabled)
            {
                Intent intent = new Intent();
                intent.SetAction(Settings.ActionRequestIgnoreBatteryOptimizations);
                intent.SetData(Android.Net.Uri.Parse("package:" + PackageName));
                StartActivityForResult(intent, 100);
            }
        }

        private bool IsBatteryOptimizationDisabled
        {
            get
            {
                PowerManager power = (PowerManager)GetSystemService(Context.PowerService);
                return power != null && power.IsIgnoringBatteryOptimizations(this.PackageName);
            }
        }

        private bool HasIngoreBatteryOptmizationPermission
        {
            get
            {
                return this.CheckSelfPermission(Manifest.Permission.RequestIgnoreBatteryOptimizations) == Permission.Granted;
            }
        }

        private void OnActivateBluetooth(object sender, EventArgs args)
        {
            var bt = Mvx.IoCProvider.Resolve<IBluetoothStreamSubsystem>();
            if (bt == null) return;
            
            if (bt.IsAvailableOnDevice && !bt.IsActivated)
            {
                Intent intentBtEnabled = new Intent(BluetoothAdapter.ActionRequestEnable);
                // The REQUEST_ENABLE_BT constant passed to startActivityForResult() is a locally defined integer (which must be greater than 0), 
                // that the system passes back to you in your onActivityResult() 
                // implementation as the requestCode parameter. 
                int REQUEST_ENABLE_BT = 1;
                StartActivityForResult(intentBtEnabled, REQUEST_ENABLE_BT);
            }   
        }

        
    }   
}