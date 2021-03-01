using System;

namespace NinjaSync.Storage
{
    public interface ITransaction : IDisposable
    {
        void Commit();
    }

    public interface  ITrackableJournalStorage : ITrackableStorage, IJournalStorage
    {
        new void Clear();
    }
}
