using System;

namespace NinjaTools.MVVM.Services
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
