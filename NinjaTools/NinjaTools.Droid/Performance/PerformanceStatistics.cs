using System;
using System.Diagnostics;
using System.IO;
using NinjaTools.Threading;

namespace NinjaTools.Droid.Performance
{
    public class PerformanceEventArgs : CpuUsageEventArgs 
    {
        public readonly ProcessStat ProcessStat;
        public readonly CpuStat CpuStat;

        public PerformanceEventArgs(float cpuPercent, ProcessStat processStat, CpuStat cpuStat)
                    :base(cpuPercent)
        {
            ProcessStat = processStat;
            CpuStat = cpuStat;
        }
    }

    public class PerformanceStatistics : IProcessCpuUsageCollector, IDisposable
    {
        private readonly bool _includeChildProcesses;
        private readonly TaskTimer _timer;

        public event EventHandler<PerformanceEventArgs> MeasurementArrived;
        public event EventHandler<CpuUsageEventArgs> CpuUsageArrived;

        private CpuStat _lastCpuStat;
        private ProcessStat _lastProcStat;

        public PerformanceStatistics(TimeSpan measurementInterval, bool includeChildProcesses = false)
        {
            _includeChildProcesses = includeChildProcesses;
            _timer = new TaskTimer(measurementInterval);
            _timer.Tick += OnTick;
        }

        public bool IsEnabled
        {
            get { return _timer.IsEnabled; }
            set { _timer.IsEnabled = value; }
        }

        public void Dispose()
        {
            _timer.Dispose();
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (MeasurementArrived == null && CpuUsageArrived == null)
            {
                IsEnabled = false;
                return;
            }

            PerformanceEventArgs args = null;

            try
            {
                var cpu = GetCpuStat();
                var proc = GetCurrentProcessStat();

                if (_lastCpuStat != null)
                {
                    // Sometimes android/linux returns invalid (or corrected?) values
                    // for Ide/IoWait. Ignore these.
                    //
                    // http://stackoverflow.com/questions/27627213/proc-stat-idle-time-decreasing
                    //
                    if (cpu.Idle >= _lastCpuStat.Idle && cpu.IoWait >= _lastCpuStat.IoWait)
                    {
                        long procTime = _includeChildProcesses ? proc.TotalChildTime : proc.TotalTime;
                        long lastProcTime = _includeChildProcesses ? _lastProcStat.TotalChildTime : _lastProcStat.TotalTime;

                        long dProc = procTime - lastProcTime;
                        long dCpu = cpu.TotalJiffies - _lastCpuStat.TotalJiffies;

                        float load = (float)dProc / dCpu;

                        args = new PerformanceEventArgs(load, proc, cpu);
                    }
                }

                _lastCpuStat = cpu;
                _lastProcStat = proc;
            }
            catch (Exception ex)
            {
                // swallow exception
                Debug.WriteLine("Error retrieving performance statistics: " + ex.ToString());
            }
            
            if (args != null)
            {
                FireMeasurementArrived(args);
                FireCpuUsageArrived(args);
            }
        }

        protected virtual bool FireMeasurementArrived(PerformanceEventArgs e)
        {
            var handler = MeasurementArrived;
            if (handler != null) handler(this, e);
            return handler != null;
        }

        protected virtual bool FireCpuUsageArrived(CpuUsageEventArgs e)
        {
            var handler = CpuUsageArrived;
            if (handler != null) handler(this, e);
            return handler != null;
        }


        public static ProcessStat GetCurrentProcessStat()
        {
            return GetProcessStat("self");
        }
        public static ProcessStat GetProcessStat(int pid)
        {
            return GetProcessStat(pid.ToStringInvariant());
        }

        private static ProcessStat GetProcessStat(string pid)
        {
            var fname = "/proc/" + pid + "/stat";
            using (var f = new FileStream(fname, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                return new ProcessStat(new StreamReader(f).ReadLine());
            }
        }

        private static CpuStat GetCpuStat()
        {
            using (var f = new FileStream("/proc/stat", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                return new CpuStat(new StreamReader(f).ReadLine());
            }
        }
    }
}
