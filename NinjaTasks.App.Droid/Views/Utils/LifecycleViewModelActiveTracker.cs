using NinjaTools.Droid.MvvmCross;
using NinjaTools.GUI.MVVM;

namespace NinjaTasks.App.Droid.Views.Utils
{
    public class LifecycleToViewModelActivation
    {
        private LifecycleState _prevLifecycle;
        
        private object _currentDataContext;
        private bool _isVisible = true;

        public bool SetDataContext(object dataContext)
        {
            if (ReferenceEquals(_currentDataContext, dataContext))
                return false;

            var prefLifecycle = _prevLifecycle;
            if(_prevLifecycle != LifecycleState.None)
                UpdateViewModelState(LifecycleState.Destroyed);

            _currentDataContext = dataContext;
            _prevLifecycle = LifecycleState.None;
            UpdateViewModelState(prefLifecycle);

            return true;
        }

        public void SetVisibility(bool isVisible)
        {
            _isVisible = isVisible;
            UpdateViewModelState(_prevLifecycle);
        }

        public void SetLifecycle(LifecycleState state)
        {
            UpdateViewModelState(state);
        }


        private void UpdateViewModelState(LifecycleState newState)
        {
            var prevState = _prevLifecycle;
            _prevLifecycle = newState;

            if (_currentDataContext == null)
                return;

            bool isVisible = _isVisible;

            if (!isVisible)
            {
                if (prevState == LifecycleState.None)
                    return;
                newState = LifecycleState.Stopped;
            }

            if (newState == prevState)
                return;

            if (newState == LifecycleState.Resumed)
            {
                var activate = _currentDataContext as IActivate;
                if (activate != null)
                    activate.OnActivate();
            }

            if (newState >= LifecycleState.Paused && prevState < LifecycleState.Paused)
            {
                var deactivate = _currentDataContext as IDeactivate;
                if (deactivate != null)
                    deactivate.OnDeactivate();
            }

            if (newState >= LifecycleState.Stopped && prevState < LifecycleState.Stopped)
            {
                var deactivate = _currentDataContext as IDeactivate;
                if (deactivate != null)
                    deactivate.OnDeactivated(false);
            }

            if (newState >= LifecycleState.Destroyed && prevState < LifecycleState.Destroyed)
            {
                var deactivate = _currentDataContext as IDeactivate;
                if (deactivate != null)
                    deactivate.OnDeactivated(true);
            }
        }

    }
}
