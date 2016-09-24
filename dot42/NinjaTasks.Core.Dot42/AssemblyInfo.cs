
using Dot42;

// For INotifyPropertyChanged implementing classes keep all public members to enable method/property databinding
[assembly: Include(Pattern = "Apply to type * when inherits('INotifyPropertyChanged'): Apply to member * when public: Dot42.Include")]
// Keep also private properties for use with code-based databinding.
[assembly: Include(Pattern = "Apply to type * when inherits('INotifyPropertyChanged'): Apply to member set_*: Dot42.Include")]
[assembly: Include(Pattern = "Apply to type * when inherits('INotifyPropertyChanged'): Apply to member get_*: Dot42.Include")]

// include types found through DI
[assembly: Include(Pattern = "Apply to type *Service: Dot42.Include")]
[assembly: Include(Pattern = "Apply to type *Factory: Dot42.Include")]
[assembly: Include(Pattern = "Apply to type *Manager: Dot42.Include")]