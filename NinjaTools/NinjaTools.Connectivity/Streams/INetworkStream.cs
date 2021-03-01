using System;
using System.Collections.Generic;
using System.Linq;

namespace NinjaTools.Connectivity.Streams
{
    public interface INetworkStream  
    {
        string        LocalIP       { get; }
        string        LocalPort     { get; }
        string        RemoteAddress { get; }
    }
}
