using System;

namespace NinjaTools.Threading
{
    public class CpuUsageEventArgs : EventArgs
    {
        /// <summary>
        /// Relative CPU usage. Value between 0 (idle) and 1 (full load).
        /// </summary>
        public readonly float CpuUsage;

        public CpuUsageEventArgs(float cpuUsage)
        {
            CpuUsage = cpuUsage;
        }
    }

    public interface IProcessCpuUsageCollector
    {
        /// <summary>
        /// enables/disables collection of CPU usage statistics.
        /// Note that the collector will auto-disable when there are no
        /// listeners, but not auto-enable.
        /// </summary>
        bool IsEnabled { get; set; }
        event EventHandler<CpuUsageEventArgs> CpuUsageArrived;
    }
}
