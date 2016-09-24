using System;
using System.IO;
using System.Text;
using NinjaTools.Logging;
using NinjaTools.Progress;

namespace TaskWarriorLib.Network
{
    public class TaskWarriorConnection : IDisposable
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private readonly Stream _s;

        public TaskWarriorConnection(Stream s)
        {
            _s = s;
        }

        public void SendRequest(TaskWarriorMsg msg)
        {
            SendRawRequest(msg.Serialize());
        }

        public TaskWarriorMsg ReceiveReply(IProgress progress)
        {
            var ret = new TaskWarriorMsg();
            string reply = ReceiveRawReply(progress);
            ret.Parse(reply);
            return ret;
        }

        public void SendRawRequest(string data)
        {
            Log.Trace("raw data send:\n"+data);
            SendBlock(Encoding.UTF8.GetBytes(data));
        }

        public string ReceiveRawReply(IProgress progress)
        {
            byte[] data = ReceiveBlock(progress);
            var reply = Encoding.UTF8.GetString(data, 0, data.Length);
            Log.Trace("raw data received:\n" + reply);
            return reply;
        }

        private void SendBlock(byte[] data)
        {
            byte[] length = new byte[4];
            var count = data.Length + 4;
            length[0] = (byte)(count >> 24);
            length[1] = (byte)(count >> 16);
            length[2] = (byte)(count >> 8);
            length[3] = (byte)(count >> 0);

            _s.Write(length, 0, 4);
            _s.Write(data, 0, data.Length);

            _s.Flush();
        }

        private byte[] ReceiveBlock(IProgress progress)
        {
            byte[] length = new byte[4];
            if(_s.Read(length, 0, 4) != 4)
                throw new IOException("connction prematurely closed.");
            progress.Progress = 0.1f;

            int len = (length[0] << 24)
                      + (length[1] << 16)
                      + (length[2] << 8)
                      + (length[3] << 0);

            len -= 4;

            if(len < 0)
                throw new IOException("connction prematurely closed.");

            byte[] data = new byte[len];

            int idx = 0;
            int remaining = len;
            while (remaining > 0)
            {
                var tryRead = Math.Min(1024,remaining);
                int read = _s.Read(data, idx, tryRead);

                if (read == 0)
                    break;

                remaining -= read;
                idx += read;

                progress.Progress = 1 - ((float) remaining/len);

            }

            if(remaining != 0) 
                throw new IOException("connction prematurely closed.");

            return data;
        }

        public void Dispose()
        {
            _s.Dispose();
        }
    }
}