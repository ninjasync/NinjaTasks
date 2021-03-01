using System;
using System.Collections.Generic;
using System.ComponentModel;
using NinjaSync.Model.Journal;
using NinjaTools.WeakEvents;

namespace NinjaSync.Obsolete
{
    /// <summary>
    /// this class collectes changes to registered classes.
    /// care has been taken to only hold weak references to
    /// the actual objects.
    /// <para/>
    /// Note: this class is not used, since it's functionality was implemented
    ///       as sqlite triggers. Keep it here as a reference, if it 
    ///       is ever needed with other storage implementations.
    /// Note: if this class ever gets used again, it should be rewritten 
    ///       to use the ConditionalWeakTable (which I did not know about
    ///          when writing this code)
    /// </summary>
    public class ModificationTracker
    {
        class Tracked
        {
            public WeakReference<ITrackable> Obj;
            public IDisposable SubscriptionToken;

            public readonly HashSet<string> Modified = new HashSet<string>();
        }

        private readonly Dictionary<JournalKey, List<Tracked>> _tracked = new Dictionary<JournalKey,List<Tracked>>();
        private readonly ShouldTrackCache _shouldTrack = new ShouldTrackCache();

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var obj = (ITrackable)sender;

            if (!_shouldTrack.ShouldTrack(sender, obj.JournalingType, e.PropertyName))
                return;

            Tracked tracked = FindObject(obj);
            if(tracked != null)
                tracked.Modified.Add(e.PropertyName);
        }

        public IList<string> GetModifications(ITrackable obj, bool clearAfterwards)
        {
            Tracked t = FindObject(obj);
            if(t == null) return new List<string>();

            var ret = t.Modified.ToList();

            if (clearAfterwards)
                t.Modified.Clear();
            
            return ret;
        }

        public void Track(ITrackable obj)
        {
            List<Tracked> l = GetList(new JournalKey(obj), false, true);
            if (FindObject(l, obj) != -1) return;

            Tracked tracked = new Tracked { Obj = new WeakReference<ITrackable>(obj) };

            l.Add(tracked);

            tracked.SubscriptionToken = WeakEventHandler<PropertyChangedEventArgs>
                                        .Register(obj, 
                                                (e, h)=>e.TrackEvent += h,
                                                (e, h) => e.TrackEvent -= h,
                                        this,   (me, sender, args) => me.OnPropertyChanged(sender, args));

            CollectGarbage();
        }

        public void StopTracking(ITrackable obj)
        {
            var key = new JournalKey(obj);
            List<Tracked> l = GetList(key, true, false);
            if (l == null) return;

            int idx = FindObject(l, obj);
            if (idx == -1) return;

            Tracked tracked = l[idx];
            tracked.SubscriptionToken.Dispose();
            l.RemoveAt(idx);

            if (l.Count == 0)
                _tracked.Remove(key);

            CollectGarbage();
        }

        private List<Tracked> GetList(JournalKey key, bool forDelete, bool forInsert)
        {
            List<Tracked> l;
            if (!_tracked.TryGetValue(key, out l))
            {
                if (forInsert)
                    return _tracked[key] = new List<Tracked>();
                return null;
            }

            RemoveStale(l);

            if (forDelete && l.Count == 0)
            {
                _tracked.Remove(key);
                return null;
            }

            return l;
        }

        private void RemoveStale(List<Tracked> l)
        {
            for (int i = l.Count - 1; i >= 0; --i)
            {
                ITrackable c;
                if (!l[i].Obj.TryGetTarget(out c))
                {
                    // remove stale objects
                    l[i].SubscriptionToken.Dispose();
                    l.RemoveAt(i);
                }
            }
        }

        private Tracked FindObject(ITrackable obj)
        {
            List<Tracked> l = GetList(new JournalKey(obj), false, false);
            if (l == null) return null;
            int idx = FindObject(l, obj);
            if (idx != -1)
                return l[idx];
            return null;
        }

        private int FindObject(IList<Tracked> l, ITrackable obj)
        {
            for (int i = l.Count - 1; i >= 0; --i)
            {
                ITrackable c;
                if (l[i].Obj.TryGetTarget(out c) && c == obj)
                    return i;
            }
            return -1;
        }

        private int _gccount;

        public void CollectGarbage()
        {
            // for now, do it every 256 operations.
            // NOTE: for lists much larger than 256, 
            // this means that operations on the tracker 
            // are are in O(n^2) or something.
            // to circumvent this, the thime could be made
            // dependent on the member length.
            if (((++_gccount) & 0xFF) != 0)
                return;

            foreach (var k in _tracked.Keys.ToList())
            {
                var list = _tracked[k];
                RemoveStale(_tracked[k]);
                if (list.Count == 0)
                    _tracked.Remove(k);
            }
            
        }

    }
}