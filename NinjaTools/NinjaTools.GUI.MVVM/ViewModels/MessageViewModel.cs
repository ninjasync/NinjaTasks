namespace NinjaTools.MVVM.ViewModels
{
    public class MessageViewModel
    {
        public string Message { get; set; }
        public string Caption { get; set; }
        public bool AllowCancel { get; set; }
        public bool YesNo { get; set; }

        public bool WasCancelled { get; set; }

        public bool IsError { get; set; }

        public MessageViewModel(string message)
        {
            Message = message;
        }

        public MessageViewModel(string caption, string message)
        {
            Message = message;
            Caption = caption;
        }

        public MessageViewModel(string caption, string messageFormat, params object[] args)
        {
            Message = string.Format(messageFormat, args);
            Caption = caption;
        }
    }
}