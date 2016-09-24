using System.Threading;
using System.Threading.Tasks;

namespace NinjaTools
{
    public static class Dot42Extensions
    {
        public static void CancelAfter(this CancellationTokenSource cancel, int delayMs)
        {
            Task.Delay(delayMs)
                .GetAwaiter().OnCompleted(cancel.Cancel);
        }
    }
}
