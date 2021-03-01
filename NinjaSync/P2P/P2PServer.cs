using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using NinjaSync.Exceptions;
using NinjaSync.Model.Journal;
using NinjaSync.P2P.Serializing;
using NinjaSync.Storage;
using NinjaTools;
using NinjaTools.Connectivity.Connections;
using NinjaTools.Connectivity.Server;
using NinjaTools.Logging;

namespace NinjaSync.P2P
{
    public class P2PServer : StreamListenServer
    {
        public override string LastError { get; protected set; }
        public int  ActiveRequests { get; private set; }

        public event EventHandler ActiveRequestsChanged;

        public event EventHandler DataDownloaded;

        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private readonly IModificationSerializer _serializer;
        private static readonly Encoding Encoding = new UTF8Encoding(false); // no BOM
        private readonly ISyncStoragesFactory _storagesFactory;
        private readonly string _logId;


        public P2PServer(IStreamListener listener, IStreamSubsystem subSystem, 
                         ISyncStoragesFactory storagesFactory, 
                         IModificationSerializer serializer,
                         string logId="p2pserver")
            : base(subSystem, listener)
        {
            _storagesFactory = storagesFactory;
            _serializer = serializer;
            _logId = logId;
        }

        protected override void HandleRequest(Stream stream, CancellationToken cancel)
        {
            // serve in background. (bluetooth tough only allows one connection at a time 
            // anyways.
            cancel.Register(stream.Dispose);
            if (stream.CanTimeout)
            {
                stream.ReadTimeout = 120000; // stop after 2 mins secs.
                stream.WriteTimeout = 120000; // stop after 2 mins secs.
            }
            else
            {
                Log.Warn("timeout not supported.");
            }

            Task.Run(() => HandleRequestImpl(stream, cancel), cancel);
        }

        private void HandleRequestImpl(Stream stream, CancellationToken cancel)
        {
            var reader = new StreamReader(stream, Encoding);
            var writer = new StreamWriter(stream, Encoding) { NewLine = "\n" };

            int numRequestsHandled = 0;

            SyncStorages storages = null;
            TrackableStorageP2PSyncEndpoint endpoint = null;

            SemaphoreSlim accountSemaphore = null;

            lock (_serializer) 
                ActiveRequests += 1;
            
            try
            {
                FireActiveRequestsChanged();

                while (!cancel.IsCancellationRequested)
                {
                    try
                    {
                        Log.Debug("{0}: handling request: waiting for request.", _logId);

                        string line;

                        // allow empty lines from previous requests.
                        while((line = reader.ReadLine()) == "" && numRequestsHandled > 0)
                            continue;
                        
                        if (line == null)
                        {
                            if (numRequestsHandled == 0)
                                throw new Exception("premature end of input");
                            return;
                        }

                        Log.Debug("{0}: request: {1}", _logId, line);

                        if (Regex.IsMatch(line, "^PUT /ninjasync HTTP/1.[01]$"))
                        {
                            SkipHeaders(reader);

                            Log.Debug("{0}: deserializing payload.", _logId);
                            CommitList remoteMod = _serializer.Deserialize(reader);

                            EnsureInitialized(ref storages, ref endpoint);

                            storages.Storage.RunInTransaction(() =>
                            {
                                var progress = new NullSyncProgress();
                                endpoint.StoreCommits(remoteMod, progress);


                                var status = storages.Status.GetStatusByRemoteStorageId(remoteMod.StorageId);
                                if (status != null)
                                {
                                    status.LocalCommitId = status.RemoteCommitId = remoteMod.FinalCommitId;
                                    status.LastSync = DateTime.UtcNow;
                                    storages.Status.SaveStatus(status);
                                }
                            });
                            

                            if (remoteMod.DeletionCount > 0 || remoteMod.ModificationCount > 0)
                                FireDataDownloaded();


                            // don't dispose.
                            Log.Debug("{0}: writing response.", _logId);
                            writer.WriteLine("HTTP/1.0 201 Created");
                            writer.WriteLine("X-NinjaSync-Version: 1.0");
                            writer.WriteLine();
                            writer.WriteLine();
                            writer.Flush();

                            
                            // stop listening after put.
                            // note: if we ever continue listening, we have to release the account here. 
                            return;
                        }
                        else if (Regex.IsMatch(line, "^GET /ninjasync HTTP/1.[01]$"))
                        {
                            SkipHeaders(reader);
                            var request = _serializer.Deserialize<GetMissingCommits>(reader);

                            if(request.StorageId.IsNullOrEmpty())
                                throw new ProtocolViolationException("missing StorageId");

                            EnsureInitialized(ref storages, ref endpoint);

                            // lock the synchronization with this account.
                            var status = storages.Status.GetStatusByRemoteStorageId(request.StorageId);
                            if (status == null)
                            {
                                Log.Info("not local account for remote StorageId {0}", request.StorageId);
                            }
                            else
                            {
                                var sem = storages.Status.GetAccountSemaphore(status.AccountId);
                                if (!sem.Wait(0))
                                {
                                    Log.Info("{0}: sync already in progress. bailing out.", status.AccountId);
                                    throw new SyncTryLaterException("reverse sync already in progress");
                                }
                                accountSemaphore = sem;
                            }


                            CommitList localCommits = endpoint.GetMissingCommits(request.LastCommonCommitId,
                                                                                 request.StorageId,
                                                                                 request.CommitIds);
                            
                            Log.Debug("{0}: writing response.", _logId);
                            writer.WriteLine("HTTP/1.0 200 OK");
                            writer.WriteLine("X-NinjaSync-Version: 1.0");
                            writer.WriteLine("Content-Type: application/json; charset=UTF-8");
                            writer.WriteLine();
                            _serializer.Serialize(writer, localCommits);
                            writer.Flush();
                        }
                        else
                            throw new Exception("Invalid request: " + line);

                        ++numRequestsHandled;
                        LastError = null;
                        Log.Debug("{0}: done.", _logId);
                    }
                    catch (CommitNotFoundException ex)
                    {
                        writer.WriteLine("HTTP/1.0 412 Precondition Failed");
                        writer.WriteLine();
                        _serializer.Serialize(writer, ex.Message);
                        writer.Flush();
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Log.Info("operation cancelled");
            }
            catch (Exception ex)
            {
                ReportError(writer, ex);
            }
            finally
            {
                // should not throw an exception, but better be on the save side.
                try { stream.Dispose(); }
                catch (Exception) { }

                if (accountSemaphore != null)
                    accountSemaphore.Release();

                lock (_serializer) ActiveRequests -= 1;
                FireActiveRequestsChanged();
            }
        }

        private void EnsureInitialized(ref SyncStorages storages, ref TrackableStorageP2PSyncEndpoint endpoint)
        {
            if (storages != null) return;
            storages = _storagesFactory.CreateSyncStorages();
            endpoint = new TrackableStorageP2PSyncEndpoint(storages.Storage);
        }

        private void SkipHeaders(StreamReader reader)
        {
            Log.Debug("{0}: handling request: skipping headers.", _logId);

            string line;
            while (!(line = reader.ReadLine()).IsNullOrEmpty())
                continue;
            if (line == null)
                throw new Exception("premature end of input.");
        }

        private void ReportError(StreamWriter writer, Exception ex)
        {
            LastError = ex.Message;

            Log.Error("{0}: error handling request from client.", _logId);
            Log.Error(ex);
            
            try
            {
                if(ex is ProtocolViolationException)
                    writer.WriteLine("HTTP/1.0 400 Bad Request");
                else
                    writer.WriteLine("HTTP/1.0 500 Internal Server Error");
                writer.WriteLine();
                _serializer.Serialize(writer, ex.GetType().Name + ": " + ex.Message);
                writer.Flush();
            }
            catch (Exception)
            {
                writer.Dispose();
            }
        }

        private void FireActiveRequestsChanged()
        {
            EventHandler handler = ActiveRequestsChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }


        private void FireDataDownloaded()
        {
            EventHandler handler = DataDownloaded;
            if (handler != null) handler(this, EventArgs.Empty);
        }

    }
}
