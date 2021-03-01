using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NinjaTools.Connectivity.Streams
{
    public interface IStreamAdapter
    {
        Stream BaseStream { get; }
    }
}
