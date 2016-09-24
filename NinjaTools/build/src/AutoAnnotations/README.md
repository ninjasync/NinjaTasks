## AutoAnnotation
This is a program to annotate Types in an Assembly with Attributes, based on specified patterns.  

It has originally been developed for usage with SmartAssembly in mind, which doesn't include such a feature out of the box. 

AutoAnnotation will search for files named *.autoannotations in the project directory, and process all patterns it finds there. The syntax is for now loosely based on Eazyfuscators syntax. This might change in the future. A better choice might be the Java-based ProGuard syntax.  

##### Show me the code

    # WPF needs to see all INotifyPropertyChanged types
    #Apply to type * when inherits('INotifyPropertyChanged'): DoNotObfuscate
    Apply to type * when inherits('INotifyPropertyChanged'): Apply to member * when public: DoNotObfuscate,DoNotPrune
    # Caliburn has to be able to match Views&ViewModels
    Apply to type *ViewModel: DoNotObfuscate
    Apply to type *View: DoNotObfuscateType,DoNotPruneType
    
    # WPF: do not prune bootstrapper
    Apply to type *.AppBootStrapper: DoNotPruneType
    
    # FluentNHibernate: must not prune any Mapping types.
    Apply to type *.Maps.*: DoNotPruneType
    
    # Model: must not prune/obfuscate any serialized or python-referenced types;
    # for nhibernate, we also have to keep the private members
    Apply to type Model.Well*: DoNotObfuscateType
    Apply to type Model.Well*: Apply to member *: DoNotObfuscate, DoNotPrune
