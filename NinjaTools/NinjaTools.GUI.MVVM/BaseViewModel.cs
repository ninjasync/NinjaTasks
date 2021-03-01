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
using MvvmCross;
using MvvmCross.Base;
using MvvmCross.Core;
using MvvmCross.ViewModels;
using NinjaTools.GUI.MVVM.Zools;

namespace NinjaTools.GUI.MVVM
{
    /// <summary>
    ///    Defines the BaseViewModel type.
    /// </summary>
    public abstract class BaseViewModel : MvxViewModel<IDictionary<string,string>>
    {
        private List<PropertyInfo> _autoBundleProperties;

        /// <summary>
        /// Gets the service.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <returns>An instance of the service.</returns>
        public TService GetService<TService>() where TService : class
        {
            return Mvx.IoCProvider.Resolve<TService>();
        }

        protected void AddToAutoBundling<T>(Expression<Func<T>> property)
        {
            if(_autoBundleProperties == null)
                _autoBundleProperties = new List<PropertyInfo>();
            _autoBundleProperties.Add(Reflection.GetPropertyInfoFromExpression(this, property));
        }
        protected void AddToAutoBundling(string propertyName)
        {
            if (_autoBundleProperties == null)
                _autoBundleProperties = new List<PropertyInfo>();
            // TODO: bind to non-public properties in base classses.
            _autoBundleProperties.Add(this.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)); 
        }

        public override void Prepare(IDictionary<string, string> parameter)
        {
            LoadAutoProperties(parameter);
        }

        protected override void InitFromBundle(IMvxBundle parameters)
        {
            base.InitFromBundle(parameters);
            LoadAutoProperties(parameters.Data);
        }

        protected override void ReloadFromBundle(IMvxBundle state)
        {
            base.ReloadFromBundle(state);
            LoadAutoProperties(state.Data);
        }

        private void LoadAutoProperties(IDictionary<string,string> data)
        {
            if (_autoBundleProperties == null || data == null) return;

            var parser = MvxSingleton<IMvxSingletonCache>.Instance.Parser;

            foreach (var prop in _autoBundleProperties)
            {
                if (!prop.CanWrite || !prop.CanRead) continue;

                string rawValue;
                if (!data.TryGetValue(prop.Name, out rawValue))
                    continue;

                object value = parser.ReadValue(rawValue, prop.PropertyType, prop.Name);
                prop.SetValue(this, value, null);
            }
        }

        protected override void SaveStateToBundle(IMvxBundle bundle)
        {
            if (_autoBundleProperties != null)
            {
                foreach (var prop in _autoBundleProperties)
                {
                    if (!prop.CanWrite || !prop.CanRead) continue;

                    bundle.Data[prop.Name] = this.GetPropertyValueAsString(prop);
                }
            }
            base.SaveStateToBundle(bundle);
        }
    }
}
