using System;
using System.Collections.Generic;
using System.Linq;

namespace NinjaTools.GUI.MVVM
{
    public interface IActivate
    {
        /// <summary>
        /// this is called before an element becomes visible (again)
        /// </summary>
        void OnActivate();
    }

    public interface IActivateByGuard
    {
        /// <summary>
        /// this is called before an element becomes visible (again), or invisible.
        /// </summary>
        Guard IsActiveGuard { get; }
    }

    public interface IDeactivate
    {
        /// <summary>
        /// this is called before an element is hidden.
        /// </summary>
        void OnDeactivate();

        /// <summary>
        /// this is called when en element was hidden or destroyed.
        /// </summary>
        /// <param name="destroying"></param>
        void OnDeactivated(bool destroying);
    }
}
