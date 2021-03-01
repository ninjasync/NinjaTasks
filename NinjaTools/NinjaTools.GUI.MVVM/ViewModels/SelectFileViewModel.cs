using System;
using System.Collections.Generic;
using System.Linq;

namespace NinjaTools.GUI.MVVM.ViewModels
{
    public enum FileMode
    {
        Save,
        Open,
        OpenMultiple
    }

    /// <summary>
    /// default mode: save.
    /// </summary>
    public class SelectFileViewModel
    {
        public FileMode Mode { get; set; }
        public string FileName { get; set; }
        public IList<string> FileNames { get; set; }

        public string Filter { get; set; }
        public bool FilterAutoAddAllFiles { get; set; } = true;

        public string Caption { get; set; }
    }
}
