using System.Collections.Generic;
using System.Linq;
using NinjaSync.Journaling;
using NinjaSync.Model;
using NinjaSync.Model.Journal;
using NinjaSync.Storage;
using NinjaTools.Progress;

namespace NinjaSync.MasterSlave
{
    /// <summary>
    /// this class wraps a slave into a master, by storing all data in an intermediate 
    /// database.
    /// 
    /// TODO: check if this class is still needed and/or functional
    /// </summary>
    public class WrapSlaveAsMasterRemoteStorage : ITrackableRemoteMasterStorage
    {
        private readonly ITrackableRemoteSlaveStorage _slave;
        private readonly SyncStorages _storages;
        private readonly string _slaveMirrorAccountId;
        private readonly LocalSlaveSyncEndpoint _mirror;
        private readonly TrackableRemoteMasterStorage _master;

        public TrackableType[] SupportedTypes { get { return _slave.SupportedTypes; } }
        public IList<string> GetMappedColums(TrackableType type){ return _slave.GetMappedColums(type); }

        public WrapSlaveAsMasterRemoteStorage(ITrackableRemoteSlaveStorage slave,
            SyncStorages intermediaSlaveStorages,
            string slaveMirrorAccountId)
        {
            _slave = slave;
            _storages = intermediaSlaveStorages;
            _slaveMirrorAccountId = slaveMirrorAccountId;
            _mirror = new LocalSlaveSyncEndpoint(slave, intermediaSlaveStorages.Storage);
            _master = new TrackableRemoteMasterStorage(_storages.Storage);
        }

        public CommitList MergeModifications(CommitList myModifications, IProgress progress)
        {
            CommitList changesForCaller=null;
            SyncStatus status = _storages.Status.GetStatus(_slaveMirrorAccountId);

            var pp = new PartitialProgress(progress);

            // (1) Drop everything into intermediate DB, enabling change tracking 
            pp.NextStep(0.3f);
            var slaveCommits = _slave.GetModifications(status.RemoteCommitId, pp);
            status.RemoteCommitId = slaveCommits.RemoteCommitId;
            
            _storages.Storage.RunInTransaction(() =>
            {
                pp.NextStep(0.1f);
                _mirror.RemoteToLocal(slaveCommits, new DefaultUpdateAlgorithm(), pp);
                string uploadChangesSinceCommitId = _storages.Storage.CommitChanges();

                // (2) merge our callers changes.
                pp.NextStep(0.1f);
                changesForCaller = _master.MergeModifications(myModifications, pp);

                // (3) collect changes for slave.
                pp.NextStep(0.1f);
                CommitList changesForSlave= new ModificationAssembler(_storages.Storage)
                                            .GetCommits(uploadChangesSinceCommitId);

                // we need to have this in the transaction as well, 
                // so if anything goes wrong we don't loose track of 
                // what still needs to be synchronized.
                pp.NextStep(0.3f); 
                _slave.SaveModifications(changesForSlave, pp);

                _storages.Status.SaveStatus(status);
            });



            return changesForCaller;
        }

        public CommitList SaveModificationsForIds(CommitList commits, IProgress progress)
        {
            foreach (var obj in commits.Commits.SelectMany(c => c.Modified).Where(p => p.Object.IsNew))
                obj.Object.SetNewId();
            return commits;
        }
    }
}
