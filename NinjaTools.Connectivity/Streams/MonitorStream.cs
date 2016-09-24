using System.IO;
using System.Text;
using NinjaTools.Logging;

namespace NinjaTools.Connectivity.Streams
{
    public class MonitorStream : StreamAdapter
    {
        private readonly bool _logTrafic;
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private readonly int _id = Unique.Create();

        public MonitorStream(Stream s, bool logTrafic = false) : base(s)
        {
            _logTrafic = logTrafic;
            Log.Trace("created " + _id);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (_logTrafic)
                Log.Trace("{0}> {1}", _id, Encoding.UTF8.GetString(buffer, offset, count));
            base.Write(buffer, offset, count);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if(_logTrafic)
                Log.Trace("{0}: request read, count={1}", _id, count);

            int read = base.Read(buffer, offset, count);

            if (_logTrafic && read > 0)
                Log.Trace("{0}< {1}", _id, Encoding.UTF8.GetString(buffer, offset, read));
            else if(_logTrafic)
                Log.Trace("{0}: 0 bytes read", _id);

            return read;
        }

        public override void Flush()
        {
            if (_logTrafic)
                Log.Trace("{0}: flush", _id);
            base.Flush();
        }

        protected override void Dispose(bool disposing)
        {
            Log.Trace("disposing {0} = {1}",_id, disposing);
            base.Dispose(disposing);
        }
    }
}
