using Dot42;
using Dot42.Manifest;
using NinjaTools.MVVM.Services;

// **********************************************************************
//
//                                Includes
//
// **********************************************************************

// For INotifyPropertyChanged implementing classes keep all public members to enable method/property databinding
[assembly: Include(Pattern = "Apply to type * when inherits('INotifyPropertyChanged'): Apply to member * when public: Dot42.Include")]
// Keep also private properties for use with code-based databinding.
[assembly: Include(Pattern = "Apply to type * when inherits('INotifyPropertyChanged'): Apply to member set_*: Dot42.Include")]
[assembly: Include(Pattern = "Apply to type * when inherits('INotifyPropertyChanged'): Apply to member get_*: Dot42.Include")]

// Keep all view types in all assemblies, possibly referenced from AXML. Some bytes might be saved
// if only the actual used types are included.
[assembly: Include(Pattern = "Apply to type * when extends('Android.Views.View'): Dot42.IncludeType", IsGlobal = true)]

// Keep MvvmCross reflection looked-up ressouce ids.
[assembly: Include(Pattern = "Apply to type *.R/*Mvx*: Dot42.IncludeType")]
[assembly: Include(Pattern = " Apply to type *.R/*: Apply to member Mvx*: Dot42.Include")]

// Keep MvvmCross ValueConverters
[assembly: Include(Pattern = "Apply to type * when implements('IMvxValueConverter'): Dot42.Include", IsGlobal = true)]

// Keep ICommand, including the event which is used by MvvmCross through reflection.
[assembly: Include(Pattern = "Apply to type System.Windows.Input.ICommand: Dot42.IncludeType", IsGlobal = true)]

// Keep bootstrapper and setup.
[assembly: Include(Pattern = "Apply to type *.Setup: Dot42.IncludeType")]
[assembly: Include(Pattern = "Apply to type * when inherits('IMvxBootstrapAction'): Dot42.IncludeType")]

// make Dot42 preserve property information for framework classes, to enable databinding on them.
[assembly: Include(Type = typeof(IncludeFrameworkProperties))]

// Include my types found through dependency injection.
[assembly: Include(Pattern = "Apply to type *Service: Dot42.Include")]
[assembly: Include(Pattern = "Apply to type *Factory: Dot42.Include")]
[assembly: Include(Pattern = "Apply to type *Manager: Dot42.Include")]

// pull in some classes without pattern.
[assembly: Include(Type = typeof(TickService))]
[assembly: Include(Type = typeof(ShowMessagesService))]
[assembly: Include(Type = typeof(MvxMessageWeakTimerService))]

// **********************************************************************
//
//                                Permissions
//
// **********************************************************************

//[assembly: UsesPermission(Android.Manifest.Permission.ACCESS_FINE_LOCATION)]
//[assembly: UsesPermission(Android.Manifest.Permission.WRITE_EXTERNAL_STORAGE)]
//[assembly: UsesPermission(Android.Manifest.Permission.READ_EXTERNAL_STORAGE)]
//[assembly: UsesPermission(Android.Manifest.Permission.BLUETOOTH_ADMIN)]
[assembly: UsesPermission(Android.Manifest.Permission.BLUETOOTH)]

//#if DEBUG
//[assembly: UsesPermission(Android.Manifest.Permission.READ_LOGS)]
//#endif