using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NinjaSync.Exceptions;
using NinjaSync.Model.Journal;
using NinjaSync.P2P.Serializing;
using NinjaTools;
using NinjaTools.Connectivity.Connections;
using NinjaTools.Logging;

namespace NinjaSync.P2P
{
    /// <summary>
    /// at the moment, this acts as a simple HTTP-like client.
    /// can be generalized in the future.
    /// </summary>
    public class P2PSyncRemoteEndpoint : IP2PSyncRemoteEndpoint, IDisposable
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private readonly IStreamConnector _connector;
        private readonly IModificationSerializer _serializer;
        private Stream _stream;
        private static readonly Encoding Encoding = new UTF8Encoding(false); // no BOM

        public P2PSyncRemoteEndpoint(IStreamConnector connector, IModificationSerializer serializer)
        {
            _connector = connector;
            _serializer = serializer;
        }

        public CommitList GetMissingCommits(string lastCommonCommitId, string myStorageId, IList<string> myCommits)
        {
            try
            {
                return GetMissingCommitsImpl(lastCommonCommitId, myStorageId, myCommits);
            }
            catch (CommitNotFoundException)
            {
                throw;
            }
            catch (Exception)
            {
                Dispose();
                throw;
            }
            
        }

        public void StoreCommits(CommitList myCommits, ISyncProgress progress)
        {
            try
            {
                StoreCommitsImpl(myCommits);
            }
            catch (Exception)
            {
                Dispose();
                throw;
            }
        }

        private CommitList GetMissingCommitsImpl(string lastCommonCommitId, string myStorageId, IList<string> myCommits)
        {
            Log.Debug("GetMissingCommits()");
            EstablishConnection();

            // do not dispose
            var w = new StreamWriter(_stream, Encoding) { NewLine = "\n" };

            w.WriteLine("GET /ninjasync HTTP/1.0");
            w.WriteLine("X-NinjaSync-Version: 1.0");
            w.WriteLine("Content-Type: application/json; charset=UTF-8");
            w.WriteLine();

            var gmc = new GetMissingCommits
            {
                LastCommonCommitId = lastCommonCommitId,
                CommitIds = myCommits,
                StorageId = myStorageId
            };
            _serializer.Serialize(w, gmc);
            w.Flush();

            Log.Debug("wrote Request");

            var reader = new StreamReader(_stream, Encoding);

            Log.Debug("waiting for response");

            string line = ReadReplyHeader(reader);
            if (!Regex.IsMatch(line, "^HTTP/1.[01] 200 "))
                throw new Exception("unexpected reponse:" + line);

            Log.Debug("deserializing payload.");

            var list = _serializer.Deserialize(reader);
            

            // set remote commit id.
            Log.Debug("done.");
            return list;
        }


        private void StoreCommitsImpl(CommitList myCommits)
        {
            Log.Debug("StoreCommits()");

            EstablishConnection();

            // do not dispose
            var w = new StreamWriter(_stream, Encoding) { NewLine = "\n" };

            w.WriteLine("PUT /ninjasync HTTP/1.0");
            w.WriteLine("X-NinjaSync-Version: v1.0");
            w.WriteLine("Content-Type: application/json; charset=UTF-8");
            w.WriteLine();
            _serializer.Serialize(w, myCommits);
            w.Flush();

            Log.Debug("wrote Request");

            var reader = new StreamReader(_stream, Encoding);

            Log.Debug("waiting for response");
            string line = ReadReplyHeader(reader);
            if (!Regex.IsMatch(line, "^HTTP/1.[01] 201 "))
                throw new Exception("unexpected reponse:" + line);
            
            // close the stream.
            Dispose();
            Log.Debug("done.");
        }

        private string ReadReplyHeader(StreamReader reader)
        {
            string header;

            // allow empty lines from previous request.
            while((header = reader.ReadLine()) == "")
                continue;

            if (header == null)
                throw new Exception("premature end of input");

            if (Regex.IsMatch(header, "^HTTP/1.[01] 412 "))
            {
                // fresh sync required?
                SkipHeaders(reader);
                string msg = _serializer.Deserialize<string>(reader);
                throw new CommitNotFoundException(null, msg);
            }
            if (!Regex.IsMatch(header, "^HTTP/1.[01] 2"))
            {
                SkipHeaders(reader);
                string msg = _serializer.Deserialize<string>(reader);
                throw new Exception(msg);
            }

            SkipHeaders(reader);
            return header;
        }

        private void EstablishConnection()
        {
            if (_stream != null)
                return;

            if (!_connector.IsAvailable)
                throw new SyncTryLaterException(TimeSpan.FromMinutes(2));

            Log.Debug("Establishing connection...");
            var task = _connector.ConnectAsync();
            _stream = task.Result;

            if (_stream.CanTimeout)
            {
                _stream.ReadTimeout = 120000; // stop after 120 secs.
                _stream.WriteTimeout = 120000; // stop after 120 secs.
            }
            else
            {
                Log.Warn("stream can not timeout");
            }

            Log.Debug("Connection established");
        }

        private static void SkipHeaders(StreamReader reader)
        {
            Log.Debug("handling reply: skipping headers.");

            string line;
            while (!(line = reader.ReadLine()).IsNullOrEmpty())
                continue;

            if (line == null)
                throw new Exception("premature end of input.");
        }

        public void Dispose()
        {
            if(_stream != null)
                _stream.Dispose();
            _stream = null;
        }
    }
}
