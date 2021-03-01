using System.Reflection;
using System.Runtime.InteropServices;
using Android.App;

[assembly: AssemblyTitle("org.ninjatasks.droid")]
[assembly: ComVisible(false)]

// Add some common permissions, these can be removed if not needed
[assembly: UsesPermission(Android.Manifest.Permission.Internet)]

[assembly: UsesPermission(Android.Manifest.Permission.WriteSyncSettings)]
[assembly: UsesPermission(Android.Manifest.Permission.ReadSyncSettings)]
[assembly: UsesPermission(Android.Manifest.Permission.ReadSyncStats)]

[assembly: UsesPermission(Android.Manifest.Permission.AuthenticateAccounts)]
[assembly: UsesPermission(Android.Manifest.Permission.GetAccounts)]
[assembly: UsesPermission(Android.Manifest.Permission.ManageAccounts)]

//[assembly: UsesPermission(Android.Manifest.Permission.ReadExternalStorage)]

[assembly: UsesPermission(Android.Manifest.Permission.Bluetooth)]
[assembly: UsesPermission(Android.Manifest.Permission.BluetoothAdmin)]
[assembly: UsesPermission(Android.Manifest.Permission.RequestIgnoreBatteryOptimizations)]
//[assembly: UsesPermission(Android.Manifest.Permission.ManageAccounts)]
//[assembly: UsesPermission(Android.Manifest.Permission.AccountManager)]

// Define Permissions before using them. [ not sure if this is actually needed...]
//[assembly: Permission(Name="org.tasks.READ")]
//[assembly: Permission(Name = "org.tasks.WRITE")]
[assembly: UsesPermission("org.tasks.READ")]
[assembly: UsesPermission("org.tasks.WRITE")]

