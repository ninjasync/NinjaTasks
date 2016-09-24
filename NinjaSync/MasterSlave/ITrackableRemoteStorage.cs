using System;
using System.Collections.Generic;
using NinjaSync.Model.Journal;
using NinjaTools.Progress;

namespace NinjaSync.Storage
{
    public interface ITrackableRemoteStorageInfo
    {

        TrackableType[] SupportedTypes { get; }

        /// <summary>
        /// can return null for a type, in which case all columns are mapped. 
        /// should return an empty list if this type is not suppoerted.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        IList<string> GetMappedColums(TrackableType type);
    }

    public interface ITrackableRemoteSlaveStorage : ITrackableRemoteStorageInfo
    {
        /// <summary>
        /// return modifications from the remote end.
        /// </summary>
        /// <param name="commitsNewerButExcludingCommitId">
        /// Can be null, in which case a full list is requested. 
        /// if not null it will be a value previously returned in 
        /// Commit.RemoteCommitIdUntil. This would allow for 
        /// incremental updates.</param>
        /// <param name="progress"></param>
        /// <returns>a modification list. if Commit.RemoteCommitIdFrom is null, this 
        /// is a full sync, i.e. returns all objects from the remote as "new".
        /// </returns>
        CommitList GetModifications(string commitsNewerButExcludingCommitId, IProgress progress);

        /// <summary>
        /// save local modifications to the remote.
        /// <para/>
        /// can throw FreshSyncRequiredException
        /// </summary>
        /// <param name="list">expects list.RemoteCommitIdFrom to be returned by 
        /// a previous call to GetModifications as RemoteCommitIdUntil, 
        /// or null for a fresh/full sync.</param>
        /// <param name="progress"></param>
        /// <returns>the same commit list as given, adjusted to what has
        /// actually been saved, dependening on the final data model.
        /// <para/>
        /// It is <strong>very important</strong> to adjust the objects in the commit list 
        /// according to your data model. It should contain what would be returned through
        /// GetModifications(). Failing to do so will result in the change-detection algorithms 
        /// to fail, breaking the update process. If in doubt, you could just return GetModifications().
        /// </returns>
        CommitList SaveModifications(CommitList list, IProgress progress);

        /// <summary>
        /// This can be the same as SaveModifications, but the caller is only
        /// interested in retrieval of ids for newly created objects, so he can
        /// continue the mapping of child objects. 
        /// <para/>
        /// Will not be called if the remote doesn't do id translation.
        /// <para/>
        /// if the remote 'HasOnlyImplicitLists', then this is used to give 
        /// the remote all lists for later retrieval of descriptions for tasks.
        /// </summary>
        /// <returns>return an empty list if the modifications where saved. 
        /// return the same list if only the ids where updated, and the actual saving
        /// is yet to be done. </returns>
        CommitList SaveModificationsForIds(CommitList commits, IProgress progress);
    }

    public interface ITrackableRemoteMasterStorage : ITrackableRemoteStorageInfo
    {
        /// <summary>
        /// this MUST be implemented if IsMergingMaster is set to true. Otherwise, it will
        /// not be called.
        /// </summary>
        /// <param name="myModifications">myModifications.RemoteCommitIdFrom should be 
        /// returned as RemoteCommitIdUntil at a previous call.</param>
        /// <param name="progress"></param>
        /// <returns>a modification list. if Commit.RemoteSyncIdFrom is null, this 
        /// is a full sync, i.e. returns all objects from the remote as "new". 
        /// <para/>
        /// RemoteSyncIdUtil  can be used to to an inceremental merge by passing it back
        /// as RemoteSyncIdFrom on a next call.
        /// <para/>
        /// SHOULD return all modifications EXCEPT the just synced ones.
        /// </returns>
        CommitList MergeModifications(CommitList myModifications, IProgress progress);

        /// <summary>
        /// This can be the same as SaveModifications, but the caller is only
        /// interested in retrieval of ids for newly created objects, so he can
        /// continue the mapping of child objects. 
        /// <para/>
        /// if the remote 'HasOnlyImplicitLists', then this is used to give 
        /// the remote all lists for later retrieval of descriptions for tasks.
        /// </summary>
        /// <returns>return an empty list if the modifications where saved. 
        /// return the same list if only the ids where updated, and the actual saving
        /// is yet to be done. </returns>
        CommitList SaveModificationsForIds(CommitList commits, IProgress progress);
    }

}
