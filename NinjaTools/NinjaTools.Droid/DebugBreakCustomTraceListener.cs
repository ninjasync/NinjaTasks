using System.Diagnostics;

namespace NinjaTools.Droid
{
    /// <summary>
    /// this class can be deleted once Xamarin supports breaking on Debug.Assert()
    /// <para/>
    /// to use: Trace.Listeners.Add(new DebugBreakTraceListener());
    /// </summary>
    public class DebugBreakTraceListener : TraceListener
    {
        public override void Fail(string message, string detailMessage)
        {
#if DEBUG
            Debugger.Break();
#endif
        }
        public override void Write(string message)
        {
        }

        public override void WriteLine(string message)
        {
        }
    }
}
