using System.IO;

namespace TaskWarriorLib.Network
{
    /// <summary>
    /// todo: move to some more generic place.
    /// </summary>
    public interface ITslConnectionFactory
    {
        Stream ConnectAndSecureFromFiles(string server, int port, string clientCertificateAndKeyPfxFile,
                                string serverCertificatePfxFile, int genericTimeoutMs=-1);
        Stream ConnectAndSecureFromPem(string server, int port, string clientCertificateAndKeyPem,
                                string serverCertificatePem, int genericTimeoutMs = -1);
    }
}
