using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using Caliburn.Micro;
using Cirrious.CrossCore;
using Cirrious.CrossCore.Exceptions;
using Cirrious.CrossCore.Platform;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.MvvmCross.Views;
using Cirrious.MvvmCross.Wpf.Views;

namespace NinjaTools.GUI.Wpf.MvvmCrossCaliburnMicro
{
    public class MyWpfPresenter : IMvxWpfViewPresenter
    {
        private readonly Window _mainWindow;

        public MyWpfPresenter(Window mainWindow)
        {
            _mainWindow = mainWindow;
        }

        public virtual void Show(MvxViewModelRequest request)
        {
            try
            {
                IMvxNativeView nativeView = LoadAsNativeView(request);

                if (nativeView != null)
                {
                    Present(nativeView);
                    return;
                }

                var loader = Mvx.Resolve<IMvxSimpleWpfViewLoader>();
                var view = loader.CreateView(request);
                Present(view);
            }
            catch (Exception exception)
            {
                MvxTrace.Error("Error seen during navigation request to {0} - error {1}", request.ViewModelType.Name,
                               exception.ToLongString());
                throw;
            }
        }

        private void Present(IMvxNativeView nativeView)
        {
            nativeView.Show();
        }

        private IMvxNativeView LoadAsNativeView(MvxViewModelRequest request)
        {
            var finder = Mvx.Resolve<IMvxViewFinder>();

            Type viewType = finder.GetViewType(request.ViewModelType);
            if (viewType == null)
                return null;

            if (!typeof (IMvxNativeView).IsAssignableFrom(viewType))
                return null;
                
            var instance = (IMvxNativeView)Activator.CreateInstance(viewType);
            if (instance == null)
                throw new MvxException("View not loaded for " + viewType);

            IMvxViewModelLoader mvxViewModelLoader = Mvx.Resolve<IMvxViewModelLoader>();
            instance.ViewModel = mvxViewModelLoader.LoadViewModel(request, null);

            return instance;
        }

        public void Present(FrameworkElement view)
        {
            IMvxView mvxView = (IMvxView)view;
            var showAsDialog = mvxView.GetType().Name.EndsWith("Dlg", StringComparison.Ordinal);

            var window = EnsureWindow(mvxView.ViewModel, mvxView, showAsDialog);

            var haveDisplayName = mvxView.ViewModel as MVVM.IHaveDisplayName;
            if (haveDisplayName != null && !ConventionManager.HasBinding(window, Window.TitleProperty))
            {
                var binding = new Binding("DisplayName") { Mode = BindingMode.TwoWay };
                window.SetBinding(Window.TitleProperty, binding);
            }

            if (window != view)
                ApplySizeConvention(window, view, showAsDialog);

            if (showAsDialog)
                window.ShowDialog();
            else
                _mainWindow.Content = view;
        }

        public void ChangePresentation(MvxPresentationHint hint)
        {
            var close = hint as MvxClosePresentationHint;
            if (close != null)
            {
                var currentMainView = _mainWindow.Content as IMvxView;
                if (currentMainView != null && currentMainView.ViewModel == close.ViewModelToClose)
                {
                    _mainWindow.Close();
                    return;
                }

                foreach (var window in Application.Current.Windows.OfType<IMvxView>())
                    if (window.ViewModel == close.ViewModelToClose)
                    {
                        ((Window) window).Close();
                        return;
                    }
                //throw new Exception("unable to close " + close.ViewModelToClose);
                MvxTrace.Warning("unable to close " + close.ViewModelToClose);  
            }
            else
            {
                MvxTrace.Warning("Hint ignored {0}", hint.GetType().Name);  
            }
        }

        private void ApplySizeConvention(Window window, FrameworkElement view, bool showAsDialog)
        {
            bool hasSize = !double.IsNaN(view.Width) && !double.IsNaN(view.Height);
            bool hasMinMaxSize = (!double.IsNaN(view.Width) || !double.IsNaN(view.MinWidth) || !double.IsNaN(view.MaxHeight))
                                && (!double.IsNaN(view.Height) || !double.IsNaN(view.MinHeight) || !double.IsNaN(view.MaxHeight));
            bool isBothDirectionsStretch = view.HorizontalAlignment == HorizontalAlignment.Stretch
                                           && view.VerticalAlignment == VerticalAlignment.Stretch;
            // If UserControl has a size specified, 
            if (hasSize && isBothDirectionsStretch && window.SizeToContent == SizeToContent.WidthAndHeight)
            {
                // stretch, but use initial size specified.
                window.Width = view.Width;
                window.Height = view.Height;
                view.Width = Double.NaN;
                view.Height = Double.NaN;
                window.SizeToContent = SizeToContent.Manual;
            }
            else if (hasMinMaxSize && isBothDirectionsStretch && window.SizeToContent == SizeToContent.WidthAndHeight)
            {
                window.Width = view.Width;
                window.Height = view.Height;
                window.MinHeight = view.MinHeight;
                window.MaxHeight = view.MaxHeight;
                window.MinWidth = view.MinWidth;
                window.MaxWidth = view.MaxWidth;
                view.Width = Double.NaN;
                view.Height = Double.NaN;
                window.SizeToContent = SizeToContent.Manual;
            }
            else if (hasSize && view.HorizontalAlignment != HorizontalAlignment.Stretch && view.VerticalAlignment != VerticalAlignment.Stretch)
            {
                // disable resizing of parent
                window.ResizeMode = ResizeMode.NoResize;
            }
            
        }

        #region from Caliburn.Micro 
        ///// <summary>
        ///// Creates a window.
        ///// </summary>
        ///// <param name="rootModel">The view model.</param>
        ///// <param name="isDialog">Whethor or not the window is being shown as a dialog.</param>
        ///// <param name="context">The view context.</param>
        ///// <param name="settings">The optional popup settings.</param>
        ///// <returns>The window.</returns>
        //protected virtual Window CreateWindow(object rootModel, bool isDialog, object context, IDictionary<string, object> settings)
        //{
        //    var view = EnsureWindow(rootModel, ViewLocator.LocateForModel(rootModel, null, context), isDialog);
        //    ViewModelBinder.Bind(rootModel, view, context);

        //    var haveDisplayName = rootModel as IHaveDisplayName;
        //    if (haveDisplayName != null && !ConventionManager.HasBinding(view, Window.TitleProperty))
        //    {
        //        var binding = new Binding("DisplayName") { Mode = BindingMode.TwoWay };
        //        view.SetBinding(Window.TitleProperty, binding);
        //    }

        //    ApplySettings(view, settings);

        //    //new WindowConductor(rootModel, view);

        //    return view;
        //}

        /// <summary>
        /// Makes sure the view is a window is is wrapped by one.
        /// </summary>
        /// <param name="model">The view model.</param>
        /// <param name="view">The view.</param>
        /// <param name="isDialog">Whethor or not the window is being shown as a dialog.</param>
        /// <returns>The window.</returns>
        protected virtual Window EnsureWindow(object model, object view, bool isDialog)
        {
            var window = view as Window;

            if (window == null)
            {
                window = new Window
                {
                    Content = view,
                    SizeToContent = SizeToContent.WidthAndHeight
                };

                window.SetValue(View.IsGeneratedProperty, true);

                var owner = InferOwnerOf(window);
                if (owner != null)
                {
                    window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    window.Owner = owner;
                }
                else
                {
                    window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                }
            }
            else
            {
                var owner = InferOwnerOf(window);
                if (owner != null && isDialog)
                {
                    window.Owner = owner;
                }
            }

            return window;
        }

        /// <summary>
        /// Infers the owner of the window.
        /// </summary>
        /// <param name="window">The window to whose owner needs to be determined.</param>
        /// <returns>The owner.</returns>
        protected virtual Window InferOwnerOf(Window window)
        {
            if (Application.Current == null)
            {
                return null;
            }

            var active = Application.Current.Windows.OfType<Window>()
                                    .FirstOrDefault(x => x.IsActive);
            active = active ?? Application.Current.MainWindow;
            return active == window ? null : active;
        }

        bool ApplySettings(object target, IEnumerable<KeyValuePair<string, object>> settings)
        {
            if (settings != null)
            {
                var type = target.GetType();

                foreach (var pair in settings)
                {
                    var propertyInfo = type.GetProperty(pair.Key);

                    if (propertyInfo != null)
                    {
                        propertyInfo.SetValue(target, pair.Value, null);
                    }
                }

                return true;
            }

            return false;
        }

        #endregion


    }
}
