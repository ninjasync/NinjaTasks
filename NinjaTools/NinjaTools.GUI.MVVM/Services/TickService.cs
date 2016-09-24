using System;
using Cirrious.MvvmCross.Plugins.Messenger;
using NinjaTools.Threading;

namespace NinjaTools.MVVM.Services
{
    public interface ITickService
    {
    }

    public class TickMessage : MvxMessage
    {
        public TickMessage(object sender)
            : base(sender)
        {
        }
    }

    public class TickService : ITickService, IDisposable
    {
        private readonly IMvxMessenger _messenger;
        private MvxSubscriptionToken _token;

        private readonly TaskTimer _timer;
        public TimeSpan TickDuration { get { return _timer.TickDuration; } set { _timer.TickDuration = value; } }

        public TickService(IMvxMessenger messenger)
        {
            _messenger = messenger;
            _token = _messenger.Subscribe<MvxSubscriberChangeMessage>(OnSubscribersChanged);
            
            _timer = new TaskTimer(TimeSpan.FromSeconds(1));
            _timer.Tick += OnTick;
            _timer.IsEnabled = _messenger.HasSubscriptionsFor<TickMessage>();
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (_messenger.HasSubscriptionsFor<TickMessage>())
                _messenger.Publish(new TickMessage(this));
        }

        private void OnSubscribersChanged(MvxSubscriberChangeMessage obj)
        {
            if (obj.MessageType != typeof(TickMessage)) return;

            _timer.IsEnabled = obj.SubscriberCount > 0;
        }

        public void Dispose()
        {
            if (_token != null)
            {
                _messenger.Unsubscribe<MvxSubscriberChangeMessage>(_token);
                _token.Dispose();
                _token = null;
            }

            _timer.Dispose();
        }
    }
}