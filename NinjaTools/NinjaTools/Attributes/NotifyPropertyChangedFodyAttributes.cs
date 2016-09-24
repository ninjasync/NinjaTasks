using System;

// ReSharper disable once CheckNamespace
namespace PropertyChanged
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class AlsoNotifyForAttribute : Attribute
    {
        public AlsoNotifyForAttribute(string property) { }
        public AlsoNotifyForAttribute(string property, params string[] otherProperties) { }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class DoNotNotifyAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class DependsOnAttribute : Attribute
    {
        public DependsOnAttribute(string dependency) { }
        public DependsOnAttribute(string dependency, params string[] otherDependencies) { }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ImplementPropertyChangedAttribute : Attribute
    {
    }

    /// <summary>
    /// Skip equality check before change notification
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class DoNotCheckEqualityAttribute : Attribute
    {
    }

    
}

