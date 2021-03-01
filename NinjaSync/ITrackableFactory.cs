using System;
using NinjaSync.Model.Journal;

namespace NinjaSync
{
    public interface ITrackableFactory
    {
        TrackableType[] Types { get; }

        string GetName(TrackableType type);

        Type GetType(TrackableType type);

        ITrackable Create(TrackableType type);
    }
}
