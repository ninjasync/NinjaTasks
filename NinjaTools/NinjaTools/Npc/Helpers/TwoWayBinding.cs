using System;

namespace NinjaTools.Npc.Helpers
{
    internal class TwoWayBinding : IDisposable
    {
        private readonly IDisposable _forward;
        private readonly IDisposable _reverse;

        public TwoWayBinding(IDisposable forward, IDisposable reverse)
        {
            _forward = forward;
            _reverse = reverse;
        }

        public void Dispose()
        {
            _forward.Dispose();
            _reverse.Dispose();
        }
    }
}