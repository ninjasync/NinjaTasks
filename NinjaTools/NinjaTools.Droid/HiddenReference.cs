using System.Collections.Generic;
using System.Threading;

namespace NinjaTools.Droid
{
    /// <summary>
    /// Used to Help the Xamarin GC, by breaking the object graph.
    /// 
    /// Whenever an instance of a Java.Lang.Object type or subclass is scanned 
    /// during the GC, the entire object graph that the instance refers to must 
    /// also be scanned. The object graph is the set of object instances that the 
    /// "root instance" refers to, plus everything referenced by what the root 
    /// instance refers to, recursively.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class HiddenReference<T>
    {
        private static readonly Dictionary<int, T> Table = new Dictionary<int, T>();
        private static int idgen = 0;

        private readonly int _id;

        public HiddenReference()
        {
            _id = Interlocked.Increment(ref idgen);
        }

        ~HiddenReference()
        {
            lock (Table)
                Table.Remove(_id);
        }

        public T Value
        {
            get { lock (Table) { return Table[_id]; } }
            set { lock (Table) { Table[_id] = value; } }
        }
    }
}