using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NinjaSync.Model.Journal;

namespace NinjaSync.Obsolete
{
    public class ShouldTrackCache
    {
        readonly Dictionary<Tuple<int, string>, bool> _cache = new Dictionary<Tuple<int, string>, bool>();

        public bool ShouldTrack(object obj, int journalType, string propertyName)
        {
            var key = new Tuple<int, string>(journalType, propertyName);
            
            bool track;
            if (_cache.TryGetValue(key, out track))
                return track;

            track = CheckAttributes(obj, propertyName);
            _cache[key] = track;
            return track;
        }

        private static bool CheckAttributes(object obj, string propertyName)
        {
            //PropertyInfo prop = obj.GetType().GetProperty(propertyName);
            PropertyInfo prop = obj.GetType().GetRuntimeProperty(propertyName);
            bool track = prop.CustomAttributes
                             .Any(p => p.AttributeType.Name == typeof (Track).Name);
            return track;
        }
    }
}
