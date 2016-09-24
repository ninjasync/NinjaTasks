using System.Threading.Tasks;
using NinjaTools.MVVM.ViewModels;

namespace NinjaTools.MVVM.Services
{
    public interface IDisplayMessageService
    {
        Task<bool> ShowDelete(MessageViewModel model);
        Task<bool> Show(MessageViewModel vm);
    }
}