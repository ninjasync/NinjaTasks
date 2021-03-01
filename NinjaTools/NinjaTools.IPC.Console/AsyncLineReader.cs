using System;
using System.Text;

namespace NinjaTools.IPC.Console
{
    public class AsyncLineReader
    {
        private readonly AsyncStreamReader _streamReader;
        private readonly LineTokenizer _lineTokenizer = new LineTokenizer();

        public event Action<string> LineRead;

        public Encoding Encoding { get; set; }

        public AsyncLineReader(AsyncStreamReader sr, Encoding encoding)
        {
            _streamReader = sr;
            _streamReader.DataRead += StreamReaderDataRead;
            Encoding = encoding;
        }

        void StreamReaderDataRead(byte[] data, int len)
        {
            if(LineRead == null) return;
            if (data == null) { LineRead(null); return; }

            string s = Encoding.GetString(data, 0, len);
            foreach (string l in _lineTokenizer.Add(s))
                LineRead(l);
        }

        public void Begin()
        {
            _streamReader.Begin();
        }

        public void End()
        {
            _streamReader.End();
        }
    }
}
