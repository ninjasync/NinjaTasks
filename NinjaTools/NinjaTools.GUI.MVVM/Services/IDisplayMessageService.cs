using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NinjaTools.GUI.MVVM.ViewModels;

namespace NinjaTools.GUI.MVVM.Services
{
    public interface IDisplayMessageService
    {
        Task<bool> ShowDelete(MessageViewModel model);
        Task<bool> Show(MessageViewModel vm);

        Task<bool> Show(InputViewModel vm);
    }
}