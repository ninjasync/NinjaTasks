using System;
using System.Collections.Generic;
using System.Linq;

namespace NinjaTools
{
    /// <summary>
    /// holds a list of IDisposables, so they don't get deleted.
    /// for easy use, has an operator+ overload.
    /// 
    /// can be used with e.g. NpcBinding.
    /// </summary>
    public class TokenBag : IDisposable
    {
        private IList<IDisposable> _references = new List<IDisposable>();

        public void Clear()
        {
            var old = _references;
            _references = new List<IDisposable>();
            foreach (var s in old)
                s.Dispose();
        }


        public void Dispose()
        {
            Clear();
        }

        public void Add(IDisposable d)
        {
            _references.Add(d);
        }
            
        public static TokenBag operator +(TokenBag bag, IDisposable d)
        {
            bag = bag ?? new TokenBag();
            bag._references.Add(d);
            return bag;
        }

    }
}
