using System.Threading;
using Java.Util.Concurrent.Atomic;

namespace NinjaTools
{
    /// <summary>
    /// Threadsafe generation of unique integer
    /// </summary>
    public class Unique
    {
        private readonly static Unique Instance = new Unique();
        public static int Create() { return Instance.Id(); }

        private readonly AtomicInteger _id = new AtomicInteger(0);

        public int Id() { return _id.IncrementAndGet(); }
    }
}
