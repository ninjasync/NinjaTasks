using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MvvmCross.IoC;

namespace NinjaTools.GUI.MVVM
{
    public static class MyMvxRegistrar
    {
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
    }
}
