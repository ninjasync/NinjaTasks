using System.Reflection;
using System.Runtime.InteropServices;

using System.Windows;
using System.Windows.Markup;

[assembly: AssemblyTitle("NinjaTools.GUI.Wpf")]

[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

//[assembly: ComVisible(false)]

[assembly: ThemeInfo(
    ResourceDictionaryLocation.None, //where theme specific resource dictionaries are located
    //(used if a resource is not found in the page, 
    // or application resource dictionaries)
    ResourceDictionaryLocation.SourceAssembly //where the generic resource dictionary is located
    //(used if a resource is not found in the page, 
    // app, or any theme specific resource dictionaries)
)]

[assembly: XmlnsPrefix("http://schemas.ninjatools.org/winfx/xaml/wpf", "ninja")]
[assembly: XmlnsDefinition("http://schemas.ninjatools.org/winfx/xaml/wpf", "NinjaTools.GUI.Wpf.Controls")]
[assembly: XmlnsDefinition("http://schemas.ninjatools.org/winfx/xaml/wpf", "NinjaTools.GUI.Wpf.Utils")]
[assembly: XmlnsDefinition("http://schemas.ninjatools.org/winfx/xaml/wpf", "NinjaTools.GUI.Wpf.Behaviors")]
[assembly: XmlnsDefinition("http://schemas.ninjatools.org/winfx/xaml/wpf", "NinjaTools.GUI.Wpf.Converter")]
//[assembly: XmlnsDefinition("http://schemas.ninjatools.org/winfx/xaml/wpf", "NinjaTools.GUI.Wpf.Views")]
