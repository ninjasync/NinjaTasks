using System;
using System.Collections.Generic;
using System.Linq;
using NinjaTools.GUI.MVVM.ViewModels;

namespace NinjaTools.GUI.MVVM.Services
{
    public interface ISelectFileService
    {
        bool Select(SelectFileViewModel model);
    }
}