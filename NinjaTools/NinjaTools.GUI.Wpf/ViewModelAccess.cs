using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using Expression = System.Linq.Expressions.Expression;

namespace NinjaTools.GUI.Wpf
{
    /// <summary>
    /// helps a View to access it's  ViewModel of a certain type.
    /// -- sometimes neccessary
    /// TODO: Separate DataContextChanged & Invokation Code (partly done!)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ViewModelAccess<T> : IWeakEventListener where T : class
    {
        public event EventHandler<ValueChangedEventArgs<T>> ModelChanged;

        private readonly FrameworkElement _element;


        public static ViewModelAccess<T> Attach(FrameworkElement attachTo)
        {
            return new ViewModelAccess<T>(attachTo);
        }

        public ViewModelAccess(FrameworkElement element)
        {
            _element = element;
            element.DataContextChanged += OnDataContextChanged;
            if (element.DataContext != null)
                OnDataContextChanged(element, new DependencyPropertyChangedEventArgs(FrameworkElement.DataContextProperty, null, element.DataContext));
        }

        public T Model { get { return _element.DataContext as T; } }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var vm = e.OldValue as INotifyPropertyChanged;
            if (vm != null)
                PropertyChangedEventManager.RemoveListener(vm, this, string.Empty);

            vm = e.NewValue as INotifyPropertyChanged;
            if (vm != null)
                PropertyChangedEventManager.AddListener(vm, this, string.Empty);

            // Notify of Model Change
            var args = new ValueChangedEventArgs<T>();
            args.OldValue = e.OldValue as T;
            args.NewValue = e.NewValue as T;
            if (ReferenceEquals(args.OldValue, args.NewValue)) return;
            var h = ModelChanged;
            if (h != null) h(this, args);
        }

        protected void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var vm = sender as T;
            if (vm == null) return;
            OnViewModelPropertyChanged(vm, e);
        }


        private readonly Dictionary<string, List<Delegate>> _registeredHandlers = new Dictionary<string, List<Delegate>>();

        private void RegisterPropertyChangedHandler(string property, Delegate propertyChangedEventHandler)
        {
            List<Delegate> list;
            if (!_registeredHandlers.TryGetValue(property, out list))
                _registeredHandlers.Add(property, list = new List<Delegate>());
            list.Add(propertyChangedEventHandler);
        }

        public void RegisterPropertyChangedHandler(string property, Action propertyChangedEventHandler)
        {
            RegisterPropertyChangedHandler(property, (Delegate)propertyChangedEventHandler);
        }

        public void RegisterPropertyChangedHandler(string property, Action<object, PropertyChangedEventArgs> propertyChangedEventHandler)
        {
            RegisterPropertyChangedHandler(property, (Delegate)propertyChangedEventHandler);
        }
        public void RegisterPropertyChangedHandler(string property, Action<T> propertyChangedEventHandler)
        {
            RegisterPropertyChangedHandler(property, (Delegate)propertyChangedEventHandler);
        }

        public void Subscribe<TProperty>(Expression<Func<T, TProperty>> property, Action propertyChangedEventHandler)
        {
            RegisterPropertyChangedHandler(GetMemberInfo(property).Name, propertyChangedEventHandler);
        }
        public void RegisterPropertyChangedHandler<TProperty>(Expression<Func<T, TProperty>> property, Action<object, PropertyChangedEventArgs> propertyChangedEventHandler)
        {
            RegisterPropertyChangedHandler(GetMemberInfo(property).Name, propertyChangedEventHandler);
        }
        public void RegisterPropertyChangedHandler<TProperty>(Expression<Func<T, TProperty>> property, Action<T> propertyChangedEventHandler)
        {
            RegisterPropertyChangedHandler(GetMemberInfo(property).Name, propertyChangedEventHandler);
        }

        protected virtual void OnViewModelPropertyChanged(T sender, PropertyChangedEventArgs e)
        {
            List<Delegate> list;
            if (!_registeredHandlers.TryGetValue(e.PropertyName, out list)) return;
            foreach (var del in list)
            {
                var parm = del as Action<object, PropertyChangedEventArgs>;
                if (parm != null)
                {
                    parm(sender, e);
                    continue;
                }

                var t = del as Action<T>;
                if (t != null) t(sender);
                else ((Action)del)();
            }
        }

        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            OnViewModelPropertyChanged(sender, (PropertyChangedEventArgs)e);
            return true;
        }

        /// <summary>
        /// Converts an expression into a <see cref="T:System.Reflection.MemberInfo"/>.
        /// 
        /// </summary>
        /// <param name="expression">The expression to convert.</param>
        /// <returns>
        /// The member info.
        /// </returns>
        private static MemberInfo GetMemberInfo(Expression expression)
        {
            LambdaExpression lambdaExpression = (LambdaExpression)expression;
            return (!(lambdaExpression.Body is UnaryExpression) ? (MemberExpression)lambdaExpression.Body : (MemberExpression)((UnaryExpression)lambdaExpression.Body).Operand).Member;
        }
    }
}