using System;
using System.Collections.Generic;
using System.Linq;
using MvvmCross.Plugin.Messenger;

namespace NinjaTools.GUI.MVVM.Services
{
    public class MvxMessageWeakTimerService : IWeakTimerService
    {
        private readonly IMvxMessenger _messenger;
        private readonly ITickService _tickService;

        public MvxMessageWeakTimerService(IMvxMessenger messenger,
                                          ITickService tickService)
        {
            _messenger = messenger;
            _tickService = tickService;
        }

        public IDisposable SubscribeWeak(Action action)
        {
            return _messenger.SubscribeOnMainThread<TickMessage>(msg => action());
        }
    }
}
