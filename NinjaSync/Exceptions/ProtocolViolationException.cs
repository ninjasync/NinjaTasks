using System;

namespace NinjaSync.Exceptions
{
    public class ProtocolViolationException : Exception
    {
        public ProtocolViolationException(string msg)
            : base(msg)
        {
            
        }
        
    }
}