using System;
using System.Collections.Generic;
using System.Linq;
using NinjaTools.Connectivity.Connections;

namespace NinjaTools.Connectivity
{
    public interface IBluetoothStreamSubsystem : IStreamSubsystem
    {
        /// <summary>
        /// when set, returned streams are wrapped in BufferedStream.
        /// </summary>
        bool UseBufferedStream { get; set; }
    }
}