using System;

namespace NinjaSync.Storage
{
  public class SyncStorages : IDisposable
  {
      public ITrackableJournalStorage Storage;
      public ISyncStatusStorage Status;

      ///// <summary>
      ///// can be null, if no mapping is required.
      ///// </summary>
      //public IStringMappingStorage Mapping;


      public virtual void Dispose()
      {
      }
  }
}
