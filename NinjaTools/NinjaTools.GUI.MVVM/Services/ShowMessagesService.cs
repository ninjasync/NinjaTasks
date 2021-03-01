using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NinjaTools.GUI.MVVM.ViewModels;

namespace NinjaTools.GUI.MVVM.Services
{
    public class ShowMessagesService : IShowMessageService
    {
        private readonly IDisplayMessageService _display;
        private readonly IConfigurationService _cfg;

        public ShowMessagesService(IDisplayMessageService display, IConfigurationService cfg)
        {
            _display = display;
            _cfg = cfg;
        }

        public Task<bool> ShowMessage(string message, bool allowCancel = false)
        {
            return _display.Show(new MessageViewModel(message) { AllowCancel = allowCancel });
        }

        public Task<bool> ShowMessage(string caption, string message)
        {
            return _display.Show(new MessageViewModel(caption, message));
        }

        public Task<bool> ShowMessage(string caption, string messageFormat, params object[] args)
        {
            return _display.Show(new MessageViewModel(caption, messageFormat, args));
        }

        public void ShowError(string msg)
        {
            _display.Show(new MessageViewModel("Error", msg) { AllowCancel = false, IsError = true });
        }

        public void ShowError(string caption, string msg)
        {
            _display.Show(new MessageViewModel(caption, msg) { AllowCancel = false, IsError = true });
        }

        public void ShowError(string caption, string msgFormat, params object[] args)
        {
            _display.Show(new MessageViewModel(caption, msgFormat, args) { AllowCancel = false, IsError = true });
        }

        public Task<bool> Confirm(string caption, string message)
        {
            return _display.Show(new MessageViewModel(caption, message) { YesNo = true });
        }

        public Task<bool> Confirm(string caption, string format, params object[] args)
        {
            return _display.Show(new MessageViewModel(caption, format, args) { YesNo = true });
        }

        public async Task ShowMessage(Type allowPermanentDisable, string caption, string format, params object[] args)
        {
            var key = "disable:" + allowPermanentDisable.FullName;

            if (_cfg.GetBool(key) == true)
                return;

            // TODO: this is a hack
            var vm = new MessageViewModel(caption, format + "\n\nDo you want to see this message next time?" , args) { YesNo = true };
            bool showAgain = await _display.Show(vm);

            if (!showAgain)
                _cfg.SetValue(key, true);
        }

        public Task<bool> ConfirmDelete(string caption, string format, params object[] args)
        {
            return _display.ShowDelete(new MessageViewModel(caption, format, args) { AllowCancel = true });
        }
    }
}
