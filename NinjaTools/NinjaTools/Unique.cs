using System.Threading;

namespace NinjaTools
{
    /// <summary>
    /// Threadsafe generation of unique integer
    /// </summary>
    public class Unique
    {
        private readonly static Unique Instance = new Unique();
        public static int Create() { return Instance.Id(); }

        private int _idgen = 0;
        public int Id() { return Interlocked.Increment(ref _idgen); }
    }
}
