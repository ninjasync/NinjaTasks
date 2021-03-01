using System;
using System.Collections.Generic;
using System.Linq;

namespace NinjaTools.GUI.MVVM.Services
{
    public interface IWeakTimerService
    {
        /// <summary>
        /// The Subscribers are collected when not used otherwise.
        /// Will call on UI Thread.
        /// </summary>
        IDisposable SubscribeWeak(Action action);
    }
}
