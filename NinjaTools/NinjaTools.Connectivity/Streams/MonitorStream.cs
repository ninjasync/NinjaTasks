using System.IO;
using System.Text;
using NinjaTools.Logging;

namespace NinjaTools.Connectivity.Streams
{
    public class MonitorStream : StreamAdapter
    {
        private readonly bool   _logTrafic;
        private readonly string _id;

        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();


        public MonitorStream(Stream s, string baseTitle = "", bool logTrafic = false, string id = null) : base(s)
        {
            _logTrafic = logTrafic;
            _id = baseTitle + (id ?? Unique.Create().ToString());
            
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
