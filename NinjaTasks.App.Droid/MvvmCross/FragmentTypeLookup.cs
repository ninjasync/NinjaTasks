// http://gregshackles.com/presenters-in-mvvmcross-navigating-android-with-fragments/

using System;
using System.Collections.Generic;
using System.Linq;
using MvvmCross.IoC;
using MvvmCross.Platforms.Android.Views.Fragments;

namespace NinjaTasks.App.Droid.MvvmCross
{
    public interface IFragmentTypeLookup
    {
        bool TryGetFragmentType(Type viewModelType, out Type fragmentType);
    }

    public class FragmentTypeLookup : IFragmentTypeLookup
    {
        private readonly IDictionary<string, Type> _fragmentLookup = new Dictionary<string, Type>(); 

        public FragmentTypeLookup()
        {
            _fragmentLookup = 
                (from type in GetType().Assembly.ExceptionSafeGetTypes ()
                    where !type.IsAbstract 
                          && !type.IsInterface 
                          && typeof(MvxFragment).IsAssignableFrom(type)
                          && type.Name.EndsWith("View")
                    select type).ToDictionary(getStrippedName);
        }

        public bool TryGetFragmentType(Type viewModelType, out Type fragmentType)
        {
            var strippedName = getStrippedName(viewModelType);

            if (!_fragmentLookup.ContainsKey(strippedName))
            {
                fragmentType = null;

                return false;
            }

            fragmentType = _fragmentLookup[strippedName];

            return true;
        }

        private string getStrippedName(Type type)
        {
            return type.Name
                .TrimEnd("View".ToCharArray())
                .TrimEnd("ViewModel".ToCharArray());
        }
    }
}
