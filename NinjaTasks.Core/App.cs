using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MvvmCross.IoC;
using MvvmCross.Navigation.EventArguments;
using MvvmCross.ViewModels;
using NinjaTasks.Core.ViewModels;

namespace NinjaTasks.Core
{
    public class App : MvxApplication
    {
        public override void Initialize()
        {
            RegisterTypesWithIoC(GetType().GetTypeInfo().Assembly);

            RegisterAppStart<AppViewModel>();
        }

        public static void RegisterTypesWithIoC(Assembly assembly)
        {
            IEnumerable<Type> list = assembly.CreatableTypes().ToList();

            list.EndingWith("Service")
                .Where(p => !p.Name.StartsWith("Mock"))
                .AsInterfaces()
                .RegisterAsLazySingleton();

            list.EndingWith("Factory")
                .Where(p => !p.Name.StartsWith("Mock"))
                .AsInterfaces()
                .RegisterAsLazySingleton();

            list.EndingWith("Manager")
                .Where(p => !p.Name.StartsWith("Mock"))
                .AsInterfaces()
                .RegisterAsLazySingleton();

            list.EndingWith("Manager")
                .Where(p => !p.Name.StartsWith("Mock"))
                .AsTypes()
                .RegisterAsLazySingleton();

            //list.EndingWith("Filter")
            //    .Where(p => !p.Name.StartsWith("Mock"))
            //    .AsInterfaces()
            //    .RegisterAsDynamic();
        }

        protected override IMvxViewModelLocator CreateDefaultViewModelLocator()
        {
            return new MyViewModelLocator();
        }

        private class MyViewModelLocator : MvxDefaultViewModelLocator
        {
            // http://stackoverflow.com/questions/16723078/mvvmcross-does-showviewmodel-always-construct-new-instances/16723459#16723459
            private WeakReference<AppViewModel> _appModel;

            public override IMvxViewModel Load(Type viewModelType, IMvxBundle parameterValues, IMvxBundle savedState, IMvxNavigateEventArgs args)
            {
                if (viewModelType == typeof(AppViewModel))
                {
                    AppViewModel ret;
                    // try to keep only a single instance of AppViewModel.
                    if (_appModel == null || (!_appModel.TryGetTarget(out ret)))
                    {
                        ret = (AppViewModel)base.Load(viewModelType, parameterValues, savedState, args);
                        _appModel = new WeakReference<AppViewModel>(ret);
                    }
                    return ret;
                }

                return base.Load(viewModelType, parameterValues, savedState, args);
            }
        }
    }
}