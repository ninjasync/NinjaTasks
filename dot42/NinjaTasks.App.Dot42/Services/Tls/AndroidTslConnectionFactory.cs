using System;
using System.IO;
using System.Threading;
using NinjaTools.Connectivity;
using NinjaTools.Connectivity.Streams;
using TaskWarriorLib.Network;

namespace NinjaTasks.App.Droid.Services.Tls
{
    class AndroidTslConnectionFactory: ITslConnectionFactory
    {
        public Stream ConnectAndSecureFromFiles(string server, int port, string clientCertificateAndKeyPfxFile,
                                                string serverCertificatePfxFile, int genericTimeout)
        {
            throw new NotImplementedException();
        }

        public Stream ConnectAndSecureFromPem(string server, int port, 
                                              string clientCertificateAndKeyPem, 
                                              string serverCertificatePem,
                                              int genericTimeoutMs)
        {
            var client = new TLSClient();
            client.Init(serverCertificatePem, clientCertificateAndKeyPem, clientCertificateAndKeyPem);

            var ret = WithTimeout.Run(genericTimeoutMs, client.Dispose, () => client.Connect(server, port));

            ret = ret.EnsureTimeoutCapable();
            ret.ReadTimeout = genericTimeoutMs;
            ret.WriteTimeout = genericTimeoutMs;
            return ret;
        }
    }
}