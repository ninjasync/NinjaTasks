using System;
using System.Collections.Generic;
using System.Linq;
using MvvmCross.Plugin.Messenger;

namespace NinjaTools.GUI.MVVM
{
    public class ErrorMessage : MvxMessage
    {
        public string Message { get; set; }
        public Exception Exception { get; set; }

        public ErrorMessage(object sender,string message)
            : base(sender)
        {
            Message = message;
        }
        public ErrorMessage(object sender, Exception ex, string message=null)
            : base(sender)
        {
            Exception = ex;
            if(message == null)
                Message = ex.Message;
        }
    }
}
