using MvvmCross.Core;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NinjaTools.GUI.MVVM
{
    public static class IMvxNavigationServiceExtensions
    {
        public static Task<bool> Navigate<TViewModel>(this IMvxNavigationService nav, object param)
            where TViewModel : IMvxViewModel<IDictionary<string, string>>
        {
            return nav.Navigate<TViewModel, IDictionary<string, string>>(param.ToSimplePropertyDictionary());
        }
    }
}
