using System;
using System.IO;
using TaskWarriorLib.Network;

namespace NinjaTasks.App.Droid.Services.Tls
{
    class AndroidTslConnectionFactory: ITslConnectionFactory
    {
        public Stream ConnectAndSecureFromFiles(string server, int port, string clientCertificateAndKeyPfxFile,
            string serverCertificatePfxFile, int genericTimeoutMs = -1)
        {
            throw new NotImplementedException();
        }

        public Stream ConnectAndSecureFromPem(string server, int port, string clientCertificateAndKeyPem, string serverCertificatePem,
                                              int genericTimeoutMs = -1)
        {
            var client = new TLSClient();
            client.Init(serverCertificatePem, clientCertificateAndKeyPem, clientCertificateAndKeyPem);

            if (genericTimeoutMs != -1 && client.Stream?.CanTimeout == true)
            {
                // TODO: check if this is working as expected.
                client.Stream.ReadTimeout = genericTimeoutMs;
                client.Stream.WriteTimeout = genericTimeoutMs;
            }
            return client.Connect(server, port);

        }
    }
}