using System;
using System.IO;
using TaskWarriorLib.Network;

namespace NinjaTasks.App.Droid.Services.Tls
{
    class AndroidTslConnectionFactory: ITslConnectionFactory
    {
        public Stream ConnectAndSecureFromFiles(string server, int port, string clientCertificateAndKeyPfxFile,
                                                string serverCertificatePfxFile)
        {
            throw new NotImplementedException();
        }

        public Stream ConnectAndSecureFromPem(string server, int port, string clientCertificateAndKeyPem, string serverCertificatePem)
        {
            var client = new TLSClient();
            client.Init(serverCertificatePem, clientCertificateAndKeyPem, clientCertificateAndKeyPem);
            return client.Connect(server, port);
        }
    }
}