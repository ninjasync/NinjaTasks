using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using NinjaTools.Logging;
using NinjaTools.Progress;
using TaskWarriorLib.Network;
using TaskWarriorLib.Parser;

namespace TaskWarriorLib
{
    public class TaskWarriorFreshSyncRequiredException : Exception
    {
        public TaskWarriorFreshSyncRequiredException(string msg) : base(msg)
        {
        }
    }

    public class TaskWarriorTryLaterException : Exception
    {
        public TaskWarriorTryLaterException(string msg)
            : base(msg)
        {
        }
    }

    /// <summary>
    /// sends data to server, receives response.
    /// </summary>
    public class TaskWarriorSyncDataExchange
    {
        private readonly ITslConnectionFactory _connectionFactory;

        private readonly ILogger _log = LogManager.GetCurrentClassLogger();

        private const int TimeoutMs = 60000;


        public TaskWarriorSyncDataExchange(ITslConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        /// <summary>
        /// returns a list of remote changed tasks, and a new syncId
        /// <para/>
        /// handles TaskWarriorTryLaterException, by waiting 5 seconds, up to
        /// two time, for a maximum of 10 seconds.
        /// </summary>
        public SyncBundle ExchangeSyncData(TaskWarriorAccount account, SyncBundle localChanges, IProgress progress)
        {
            int repeatCount;

            for (repeatCount = 0; repeatCount < 3; ++repeatCount)
            {
                try
                {
                    return ExchangeSyncDataOnce(account, localChanges, progress);
                }
                catch (TaskWarriorTryLaterException)
                {
                    Task.Delay(5000).Wait();
                    continue;
                }
                // DONT handle TaskWarriorFreshSyncRequiredException here, since we do not
                //      have enough information which changed Tasks should be send after
                //      a fresh sync.
                //catch (TaskWarriorFreshSyncRequiredException)
                //{
                //    // synchronize with empty sync.
                //    var bundleFull = ExchangeSyncDataOnce(account, new SyncBundle(), progress);
                //    var bundleSecond = new SyncBundle { ChangedTasks = localChanges.ChangedTasks, SyncId = bundleFull.SyncId};
                //    continue;
                //}
            }

            throw new IOException("server busy. try again later.");
            //throw new IOException("unable to sync after multiple tries.");
        }

        /// <summary>
        /// returns a list of remote changed tasks, and a new syncId
        /// 
        /// can throw TaskWarriorFreshSyncRequiredException and TaskWarriorTryLaterException
        /// </summary>
        public SyncBundle ExchangeSyncDataOnce(TaskWarriorAccount account, 
                                                SyncBundle localChanges,
                                                IProgress  progress)
        {
            string mySyncId = localChanges.SyncId;
            
            if (mySyncId == Guid.Empty.ToString())
                mySyncId = null;

            string errmsg;
            ErrorCode code = ErrorCode.Success;

            using (var con = Connect(account, new PartitialProgress(progress, 0, 0.2f)))
            {
                var reqMsg = CreateRequest(account, localChanges.ChangedTasks, mySyncId);
                    
                _log.Debug("sending request {0} syncId; {1} locally changed tasks", string.IsNullOrEmpty(mySyncId)?"w/o":"w/", localChanges.ChangedTasks.Count);
                progress.Title = "sending synchronization request";
                con.SendRequest(reqMsg);

                _log.Debug("receiving reply...");
                progress.Title = "receiving reply";
                var replMsg = con.ReceiveReply(new PartitialProgress(progress, 0.2f, 0.8f));
                    
                int replyCode;

                if (!int.TryParse(replMsg.GetHeader("code") ?? "400", out replyCode))
                    throw new IOException("malformed data.");

                code = ErrorConverter.Convert(replyCode, replMsg.GetHeader("status"), out errmsg);
                _log.Info("reply: {0}/{3}: {1}/{2}", replyCode, code, errmsg, replMsg.GetHeader("status"));

                if (code == ErrorCode.TryLater)
                {
                    _log.Info("server busy. should try later: {0}", replMsg.GetHeader("status"));
                    throw new TaskWarriorTryLaterException(replMsg.GetHeader("status"));
                }

                if (code == ErrorCode.CouldNotFindCommonAncestor
                        || code == ErrorCode.ClientSyncKeyNotFound)
                {
                    _log.Warn("fresh sync required: {0}/{3}: {1}/{2}", replyCode, code, errmsg, replMsg.GetHeader("status"));
                    throw new TaskWarriorFreshSyncRequiredException(errmsg + ": " + replMsg.GetHeader("status"));
                }

                if (code != ErrorCode.Success && code != ErrorCode.SuccessNoChanges)
                    throw new IOException(errmsg + ": " + replMsg.GetHeader("status"));
                    
                progress.Title = "parsing reply";
                var ret = new SyncBundle();
                ret.ChangedTasks = UnpackReply(replMsg.Payload, out ret.SyncId, new PartitialProgress(progress, 0.8f, 1));

                if (ret.SyncId == null && code != ErrorCode.SuccessNoChanges)
                    throw new IOException("unable to receive syncId");
                
                // if you delete the tx.data file in taskwarrior [e.g. because it became corrupted], 
                // taskwarrior will not return a sync-id. this works around that. 
                // This special handling could be removed once taskwarrior adheres to its own 
                // protocol.
                if (ret.SyncId == null)
                    ret.SyncId = Guid.Empty.ToString();

                return ret;
            }
        }

        private IList<TaskWarriorTask> UnpackReply(string payload, out string syncId, IProgress progress)
        {
            IList<TaskWarriorTask> ret = new List<TaskWarriorTask>();
            string[] lines = payload.Split('\n');
            syncId = null;
            var parser = new TaskWarriorTaskParser();
            int idx = 0;
            foreach (var line in lines)
            {
                ++idx;
                if (idx%20 == 0) progress.Progress = (float) idx/lines.Length;

                if (string.IsNullOrEmpty(line)) continue;

                if (line.StartsWith("{"))
                {
                    try
                    {
                        var task = parser.Parse(line);
                        ret.Add(task);
                    }
                    catch (Exception)
                    {
                        _log.Error("unable to parse line: {0}", line);
                    }
                }
                else
                {
                    syncId = line.Trim();
                    break;
                }
            }
            return ret;
        }

        private static TaskWarriorMsg CreateRequest(TaskWarriorAccount account, IEnumerable<TaskWarriorTask> localChanged, string syncId)
        {
            TaskWarriorMsg msg = new TaskWarriorMsg();

            msg.Set("protocol", "v1");
            msg.Set("type", "sync");
            msg.Set("org", account.Org);
            msg.Set("user", account.User);
            msg.Set("key", account.Key);

            StringBuilder payload = new StringBuilder();

            if (!string.IsNullOrEmpty(syncId))
                payload.Append(syncId);

            var parser = new TaskWarriorTaskParser();

            foreach (var task in localChanged)
            {
                if (payload.Length != 0) payload.Append("\n");

                var obj = parser.ToRepr(task);
                payload.Append(obj);
            }

            msg.Payload = payload.ToString();
            return msg;
        }

        private TaskWarriorConnection Connect(TaskWarriorAccount account, IProgress progress)
        {
            progress.Progress = 0.1f;
            progress.Title = string.Format("Connecting to {0}:{1}", account.ServerHostname, account.ServerPort);

            Stream stream;
            if (!string.IsNullOrEmpty(account.ClientCertificateAndKeyPem))
            {
                // use PEM
                stream = _connectionFactory.ConnectAndSecureFromPem(account.ServerHostname, account.ServerPort,
                                     account.ClientCertificateAndKeyPem, account.ServerCertificatePem, TimeoutMs);
            }
            else
            {
                // use files.
                stream = _connectionFactory.ConnectAndSecureFromFiles(account.ServerHostname, account.ServerPort,
                                     account.ClientCertificateAndKeyPfxFile, account.ServerCertificateCrtFile, TimeoutMs);
            }


            return new TaskWarriorConnection(stream);
        }


    }
}
