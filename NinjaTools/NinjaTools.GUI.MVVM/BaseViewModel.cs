// --------------------------------------------------------------------------------------------------------------------
// <summary>
//    Defines the BaseViewModel type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Cirrious.CrossCore;
using Cirrious.CrossCore.Core;
using Cirrious.MvvmCross;
using Cirrious.MvvmCross.Platform;
using Cirrious.MvvmCross.ViewModels;
using NinjaTools.MVVM.Zools;

namespace NinjaTools.MVVM
{
    /// <summary>
    ///    Defines the BaseViewModel type.
    /// </summary>
    public abstract class BaseViewModel : MvxViewModel
    {
        private List<PropertyInfo> _autoBundleProperties;

        /// <summary>
        /// Gets the service.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <returns>An instance of the service.</returns>
        public TService GetService<TService>() where TService : class
        {
            return Mvx.Resolve<TService>();
        }
#if !DOT42
        protected void AddToAutoBundling<T>(Expression<Func<T>> property)
        {
            if(_autoBundleProperties == null)
                _autoBundleProperties = new List<PropertyInfo>();
            _autoBundleProperties.Add(Reflection.GetPropertyInfoFromExpression(this, property));
        }
#else
        protected void AddToAutoBundling(string propertyName)
        {
            if (_autoBundleProperties == null)
                _autoBundleProperties = new List<PropertyInfo>();
            _autoBundleProperties.Add(this.GetType().GetProperty(propertyName, BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance));
        }
#endif

        protected override void InitFromBundle(IMvxBundle parameters)
        {
            base.InitFromBundle(parameters);
            if (_autoBundleProperties == null) return;

            foreach (var propName in _autoBundleProperties.Select(p=>p.Name)
                                                          .Intersect(parameters.Data.Keys))
            {
                var prop = _autoBundleProperties.First(p => p.Name == propName);
                if (!prop.CanWrite) continue;
                
                string rawValue = parameters.Data[propName];
                object value = MvxSingleton<IMvxSingletonCache>.Instance
                                    .Parser.ReadValue(rawValue, prop.PropertyType, propName);

                prop.SetValue(this, value, null);
            }
        }

        protected override void SaveStateToBundle(IMvxBundle bundle)
        {
            if (_autoBundleProperties != null) 
                foreach (var prop in _autoBundleProperties)
                {
                    if (!prop.CanWrite || !prop.CanRead) continue;

                    bundle.Data[prop.Name] = this.GetPropertyValueAsString(prop);
                }

            base.SaveStateToBundle(bundle);
        }

        protected override void ReloadFromBundle(IMvxBundle state)
        {
            base.ReloadFromBundle(state);
            
            if (_autoBundleProperties == null) return;

            foreach (var prop in _autoBundleProperties)
            {
                if (!prop.CanWrite || !prop.CanRead) continue;

                string rawValue;
                if(!state.Data.TryGetValue(prop.Name, out rawValue))
                    continue;
                
                object value = MvxSingleton<IMvxSingletonCache>.Instance
                                    .Parser.ReadValue(rawValue, prop.PropertyType, prop.Name);
                prop.SetValue(this, value, null);
            }
        }
    }
}
