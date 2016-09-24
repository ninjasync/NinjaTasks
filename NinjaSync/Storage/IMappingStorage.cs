using NinjaSync.Model;
using NinjaSync.Model.Journal;

namespace NinjaSync.Storage
{
    /// <summary>
    /// this service maps between localIds and remoteIds.
    /// </summary>
    /// <typeparam name="TRemoteId"></typeparam>
    public interface IMappingStorage<TRemoteId> 
    {
        IdMap<TRemoteId> GetMappingFromLocal(TrackableType objectType, string localId);
        IdMap<TRemoteId> GetMappingFromRemote(TrackableType objectType, TRemoteId remoteid);

        //IEnumerable<IdMap<TRemoteId>> GetMappings();

        void SaveMapping(IdMap<TRemoteId> mapping);
        void SaveMapping(TrackableType objectType, string localId, TRemoteId remoteId);
    }

    public interface IIntMappingStorage : IMappingStorage<int>
    {
    }

    public interface IStringMappingStorage : IMappingStorage<string>
    {
    }
}
