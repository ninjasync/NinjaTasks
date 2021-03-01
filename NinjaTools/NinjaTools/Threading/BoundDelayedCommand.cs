using System;
using System.Collections.Generic;
using System.Text;

namespace NinjaTools.Threading
{
    public class BoundDelayedCommand
    {
        private readonly Action _a;
        private DelayedCommand _command = new DelayedCommand();

        public BoundDelayedCommand(Action a)
        {
            _a = a;
        }

        public void Schedule()
        {
            _command.Schedule(_a);
        }
    }
}
