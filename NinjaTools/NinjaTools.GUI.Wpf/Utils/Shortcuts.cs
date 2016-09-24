using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
using Caliburn.Micro;
using TriggerBase = System.Windows.Interactivity.TriggerBase;

namespace NinjaTools.GUI.Wpf.Utils
{
    public static class ShortcutParser
    {
        private static int _shortcutInitialized;

        /// <summary>
        /// Initializes the 'micro:Message.Attach="[Shortcut F6]=[Action XYZ]"' parsing.
        /// </summary>
        public static void InitializeShortcuts()
        {
            if ((_shortcutInitialized++) > 0) return;

            // Initialize Parsing System
            var currentParser = Parser.CreateTrigger;
            Parser.CreateTrigger = (target, triggerText) => ShortcutParser.CanParse(triggerText)
                                                                ? ShortcutParser.CreateTrigger(triggerText)
                                                                : currentParser(target, triggerText);

            // Do not update availability if a shortcut is attached to the message.
            var currentAvailability = ActionMessage.ApplyAvailabilityEffect;
            ActionMessage.ApplyAvailabilityEffect = context =>
            {
                if (!(bool)context.Message.GetValue(UnguardedInputBindingTrigger.DisableGuardProperty))
                    return currentAvailability(context);

                // enable guard enforcing the first time shortcuts are used
                ActionMessage.EnforceGuardsDuringInvocation = true;

                if (ConventionManager.HasBinding(context.Source, UIElement.IsEnabledProperty))
                    return context.Source.IsEnabled;
                return context.CanExecute == null || context.CanExecute();
            };
        }

        public static bool CanParse(string triggerText)
        {
            return !string.IsNullOrWhiteSpace(triggerText) && triggerText.Contains("Shortcut");
        }

        public static TriggerBase CreateTrigger(string triggerText)
        {
            var triggerDetail = triggerText
                .Replace("[", string.Empty)
                .Replace("]", string.Empty)
                .Replace("Shortcut", string.Empty)
                .Trim();

            var keyBinding = ParseShortcut(triggerDetail);
            var trigger = new UnguardedInputBindingTrigger { InputBinding = keyBinding };
            return trigger;
        }

        public static KeyBinding ParseShortcut(string triggerDetail)
        {
            var modKeys = ModifierKeys.None;

            var allKeys = triggerDetail.Split('+');
            var key = (Key) Enum.Parse(typeof (Key), allKeys.Last());

            foreach (var modifierKey in allKeys.Take(allKeys.Count() - 1))
            {
                modKeys |= (ModifierKeys) Enum.Parse(typeof (ModifierKeys), modifierKey);
            }

            var keyBinding = new KeyBinding(new UnguardedInputBindingTrigger(), key, modKeys);
            return keyBinding;
        }
    }

    public class UnguardedInputBindingTrigger : TriggerBase<FrameworkElement>, ICommand
    {
        public static readonly DependencyProperty InputBindingProperty =
          DependencyProperty.Register("InputBinding", typeof(InputBinding)
            , typeof(UnguardedInputBindingTrigger)
            , new UIPropertyMetadata(null));

        public InputBinding InputBinding
        {
            get { return (InputBinding)GetValue(InputBindingProperty); }
            set { SetValue(InputBindingProperty, value); }
        }

        public static readonly DependencyProperty DisableGuardProperty =
            DependencyProperty.Register("DisableGuard", typeof(bool), typeof(UnguardedInputBindingTrigger), new PropertyMetadata(default(bool)));
        public bool DisableGuard { get { return (bool)GetValue(DisableGuardProperty); } set { SetValue(DisableGuardProperty, value); } }

        public UnguardedInputBindingTrigger()
        {
            this.Actions.Changed += OnActionAdded;
        }

        private void OnActionAdded(object sender, EventArgs e)
        {
            foreach(var action in Actions)
                action.SetValue(DisableGuardProperty, true);
        }

        public event EventHandler CanExecuteChanged = delegate { };

        public bool CanExecute(object parameter)
        {
            // action is anyway blocked by Caliburn at the invoke level
            return true;
        }

        public void Execute(object parameter)
        {
            InvokeActions(parameter);
        }

        protected override void OnAttached()
        {
            if (InputBinding != null)
            {
                InputBinding.Command = this;
                if (AssociatedObject.Focusable)
                {
                    AssociatedObject.InputBindings.Add(InputBinding);
                }
                else
                {
                    Window window = null;
                    AssociatedObject.Loaded += delegate
                    {
                        window = GetWindow(AssociatedObject);
                        if (!window.InputBindings.Contains(InputBinding))
                        {
                            window.InputBindings.Add(InputBinding);
                        }
                    };
                    // TODO: check if this is needed.
                    AssociatedObject.Unloaded += delegate
                    {
                        window.InputBindings.Remove(InputBinding);
                    };
                }
            }
            base.OnAttached();
        }

        private Window GetWindow(FrameworkElement frameworkElement)
        {
            if (frameworkElement is Window)
                return frameworkElement as Window;

            var parent = frameworkElement.Parent as FrameworkElement;
            Debug.Assert(parent != null);

            return GetWindow(parent);
        }
    }
}
