using System;
using System.Threading.Tasks;

namespace NinjaTools.MVVM.Services
{
    public interface IShowMessageService
    {
        Task<bool> ShowMessage(string message, bool allowCancel = false);
        Task<bool> ShowMessage(string caption, string message);
        Task<bool> ShowMessage(string caption, string messageFormat, params object[] args);
        Task ShowMessage(Type allowPermanentDisable, string caption, string format, params object[] args);

        void ShowError(string msg);
        void ShowError(string caption, string msg);
        void ShowError(string caption, string msgFormat, params object[] args);

        Task<bool> Confirm(string caption, string message);
        Task<bool> Confirm(string caption, string format, params object[] args);
        

        Task<bool> ConfirmDelete(string caption, string message, params object[] args);
    }
}
