using System;
using System.ComponentModel;

namespace NinjaTools.WeakEvents
{
    /// <summary>
    /// Helper class to add weak handlers to events of type System.ComponentModel.PropertyChangedEventHandler.
    /// </summary>
#if !DOT42 
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
#endif
    public static class PropertyChangedWeakEventHandler
    {
        /// <summary>
        /// Registers an event handler that works with a weak reference to the target object.
        /// Access to the event and to the real event handler is done through lambda expressions.
        /// The code holds strong references to these expressions, so they must not capture any
        /// variables!
        /// </summary>
        /// <example>
        /// <code>
        /// PropertyChangedWeakEventHandler.Register(
        /// 	textDocument,
        /// 	this,
        /// 	(me, sender, args) => me.OnDocumentChanged(sender, args));
        /// </code>
        /// to unsubscribe, you can Dispose the returned object.
        /// </example>
        public static WeakEventHandler Register<TEventListener>(
            INotifyPropertyChanged source, TEventListener listeningObject,
            Action<TEventListener, object, PropertyChangedEventArgs> forwarderAction
        )
            where TEventListener : class
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (listeningObject == null)
                throw new ArgumentNullException("listeningObject");
            WeakEventHandler.VerifyDelegate(forwarderAction, "forwarderAction");

            WeakEventHandler weh = new WeakEventHandler(listeningObject);
            PropertyChangedEventHandler eh = MakeDeregisterCodeAndWeakEventHandler(weh, source, forwarderAction);
            source.PropertyChanged += eh;
            return weh;
        }

        static PropertyChangedEventHandler MakeDeregisterCodeAndWeakEventHandler<TEventListener>
            (
                WeakEventHandler weh,
                INotifyPropertyChanged senderObject,
                Action<TEventListener, object, PropertyChangedEventArgs> forwarderAction
            )
            where TEventListener : class
        {
            PropertyChangedEventHandler eventHandler = (sender, args) =>
            {
                TEventListener listeningObject = (TEventListener)weh.listeningReference.Target;
                if (listeningObject != null)
                {
                    forwarderAction(listeningObject, sender, args);
                }
                else
                {
                    weh.Dispose();
                }
            };

            weh.deregisterCode = delegate { senderObject.PropertyChanged -= eventHandler; };
            return eventHandler;
        }
    }
}
