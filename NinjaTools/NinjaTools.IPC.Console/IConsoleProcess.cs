using System;

namespace NinjaTools.IPC.Console
{
    public enum CommunicationMode
    {
        None,
        Line,
        Binary,
    }

    public enum CommunicationSource
    {
        Stdout,
        Stderr,
    }

    public interface IConsoleProcess
    {
        string AppFileName { get; set; }
        string Arguments { get; set; }
        string WorkingDirectory { get; set; }
        CommunicationMode ModeStdout { get; set; }
        CommunicationMode ModeStderr { get; set; }

        void Start();

        bool IsRunning { get; }

        event Action<string, CommunicationSource> LineRead;
        event Action<byte[], int, CommunicationSource> DataRead;

        void StdinWrite(string msg);

        int ExitCode { get; }
        event EventHandler Exited;

        /// <summary>
        /// will close input stdout and stderr streams, 
        /// usually resulting in termination
        /// of console application
        /// 
        /// will not raise exceptions
        /// </summary>
        void CloseStreams(int msWaitForExitCode = 0);
        
        void Kill();
        void Wait();
    }
}