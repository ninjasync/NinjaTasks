using NinjaTools.MVVM.ViewModels;

namespace NinjaTools.MVVM.Services
{
    public interface ISelectFileService
    {
        bool Select(SelectFileViewModel model);
    }
}