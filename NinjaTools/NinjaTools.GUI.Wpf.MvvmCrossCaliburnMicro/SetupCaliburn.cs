using MvvmCross;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NinjaTools.GUI.Wpf.MvvmCrossCaliburnMicro
{
    public static class SetupCaliburn
    {
        public static void SetupIoC()
        {
            Caliburn.Micro.IoC.GetInstance = (type, key) => Mvx.IoCProvider.Resolve(type);
            Caliburn.Micro.IoC.GetAllInstances = (type) => new[] { Mvx.IoCProvider.Resolve(type) };
            Caliburn.Micro.IoC.BuildUp = (obj) => { };
        }
    }
}
