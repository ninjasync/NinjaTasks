using System;
using System.Globalization;
using Android.Util;

namespace NinjaTools.Droid.Performance
{
    public class CpuStat
    {
        public readonly long User;
        public readonly long Nice;
        public readonly long System;
        public readonly long Idle;
        public readonly long IoWait;
        public readonly long Irq;
        public readonly long SoftIrq;
        public readonly long Steal;

        public long TotalJiffies { get { return User + Nice + System + Idle + IoWait + Irq + SoftIrq + Steal; } }

        public CpuStat(string statOutput)
        {
            var l = statOutput.Split(new [] {' '}, StringSplitOptions.RemoveEmptyEntries);

            User   = long.Parse(l[1]);
            Nice   = long.Parse(l[2]);
            System = long.Parse(l[3]);
            Idle   = long.Parse(l[4]);
            IoWait = long.Parse(l[5]);
            Irq    = long.Parse(l[6]);
            SoftIrq= long.Parse(l[7]);
            Steal  = long.Parse(l[8]);
        }

        // 1.8 Miscellaneous kernel statistics in /proc/stat
        // -------------------------------------------------
	       
        // Various pieces   of  information about  kernel activity  are  available in the
        // /proc/stat file.  All  of  the numbers reported  in  this file are  aggregates
        // since the system first booted.  For a quick look, simply cat the file:
	       
        //   > cat /proc/stat
        //   cpu  2255 34 2290 22625563 6290 127 456 0 0
        //   cpu0 1132 34 1441 11311718 3675 127 438 0 0
        //   cpu1 1123 0 849 11313845 2614 0 18 0 0
        //   intr 114930548 113199788 3 0 5 263 0 4 [... lots more numbers ...]
        //   ctxt 1990473
        //   btime 1062191376
        //   processes 2915
        //   procs_running 1
        //   procs_blocked 0
        //   softirq 183433 0 21755 12 39 1137 231 21459 2263
	       
        // The very first  "cpu" line aggregates the  numbers in all  of the other "cpuN"
        // lines.  These numbers identify the amount of time the CPU has spent performing
        // different kinds of work.  Time units are in USER_HZ (typically hundredths of a
        // second).  The meanings of the columns are as follows, from left to right:
	       
        // - user: normal processes executing in user mode
        // - nice: niced processes executing in user mode
        // - system: processes executing in kernel mode
        // - idle: twiddling thumbs
        // - iowait: waiting for I/O to complete
        // - irq: servicing interrupts
        // - softirq: servicing softirqs
        // - steal: involuntary wait
        // - guest: running a normal guest
        // - guest_nice: running a niced guest

    }
}