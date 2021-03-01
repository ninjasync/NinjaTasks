using System;
using Android.Util;

namespace NinjaTools.Droid.Performance
{
    public class ProcessStat
    {
        /// <summary>
        /// user mode jiffies
        /// </summary>
        public readonly long UTime;
        /// <summary>
        /// kernel mode jiffies
        /// </summary>
        public readonly long STime;
            
        /// <summary>
        /// user mode jiffies with child's
        /// </summary>
        public readonly long CUTime;

        /// <summary>
        /// kernel mode jiffies with child's
        /// </summary>
        public readonly long CSTime;

        /// <summary>
        /// returns UTime + STime
        /// </summary>
        public long TotalTime { get { return UTime + STime; } }

        /// <summary>
        /// returns CUTime + CSTime
        /// </summary>
        public long TotalChildTime { get { return CUTime + CSTime; } }

        /// <summary>
        /// number of threads
        /// </summary>
        public readonly long NumThreads;

        /// <summary>
        /// virtual memory size
        /// </summary>
        public readonly long VSize;

        public ProcessStat(string statOutput)
        {
            var l = statOutput.Split(new [] {' ', '\r', '\n', '\t'}, StringSplitOptions.RemoveEmptyEntries);
            UTime = Int64.Parse(l[13]);
            STime = Int64.Parse(l[14]);
            CUTime = Int64.Parse(l[15]);
            CSTime = Int64.Parse(l[16]);
            NumThreads = Int64.Parse(l[19]);
            VSize = Int64.Parse(l[22]);
        }

        // Table 1-4: Contents of the stat files (as of 2.6.30-rc7)
        //..............................................................................
        // Field          Content
        //  pid           process id
        //  tcomm         filename of the executable
        //  state         state (R is running, S is sleeping, D is sleeping in an
        //                uninterruptible wait, Z is zombie, T is traced or stopped)
        //  ppid          process id of the parent process
        //  pgrp          pgrp of the process
        //  sid           session id
        //  tty_nr        tty the process uses
        //  tty_pgrp      pgrp of the tty
        //  flags         task flags
        //  min_flt       number of minor faults
        //  cmin_flt      number of minor faults with child's
        //  maj_flt       number of major faults
        //  cmaj_flt      number of major faults with child's
        //  utime         user mode jiffies
        //  stime         kernel mode jiffies
        //  cutime        user mode jiffies with child's
        //  cstime        kernel mode jiffies with child's
        //  priority      priority level
        //  nice          nice level
        //  num_threads   number of threads
        //  it_real_value	(obsolete, always 0)
        //  start_time    time the process started after system boot
        //  vsize         virtual memory size
        //  rss           resident set memory size
        //  rsslim        current limit in bytes on the rss
        //  start_code    address above which program text can run
        //  end_code      address below which program text can run
        //  start_stack   address of the start of the main process stack
        //  esp           current value of ESP
        //  eip           current value of EIP
        //  pending       bitmap of pending signals
        //  blocked       bitmap of blocked signals
        //  sigign        bitmap of ignored signals
        //  sigcatch      bitmap of caught signals
        //  wchan         address where process went to sleep
        //  0             (place holder)
        //  0             (place holder)
        //  exit_signal   signal to send to parent thread on exit
        //  task_cpu      which CPU the task is scheduled on
        //  rt_priority   realtime priority
        //  policy        scheduling policy (man sched_setscheduler)
        //  blkio_ticks   time spent waiting for block IO
        //  gtime         guest time of the task in jiffies
        //  cgtime        guest time of the task children in jiffies
        //  start_data    address above which program data+bss is placed
        //  end_data      address below which program data+bss is placed
        //  start_brk     address above which program heap can be expanded with brk()
        //  arg_start     address above which program command line is placed
        //  arg_end       address below which program command line is placed
        //  env_start     address above which program environment is placed
        //  env_end       address below which program environment is placed
        //  exit_code     the thread's exit_code in the form reported by the waitpid system call
        //..............................................................................

    }
}