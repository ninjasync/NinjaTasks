using System;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using NinjaTools.Logging;
using TaskWarriorLib.Network;

namespace NinjaTasks.App.Wpf.Services.TcpIp
{
    public class TslConnectionFactory : ITslConnectionFactory
    {
        private readonly ILogger _log = LogManager.GetCurrentClassLogger();

        private const bool CheckCertificateRevocation = false;

        private const SslProtocols EnabledSslProtocols = SslProtocols.Tls
#if !XAMARIN_DROID
            | SslProtocols.Tls11 | SslProtocols.Tls12
#else
#endif
            ;

        public Stream ConnectAndSecureFromPem(string server, int port, string clientCertificateAndKeyPem,
                                              string serverCertificatePem, int genericTimeoutMs)
        {
            throw new NotImplementedException("please specify a .pfx file");

            //  _log.Debug("Parsing Cert/Key");
            //var clientCert = PemImport1.FromPem(clientCertificateAndKeyPem, "");
            //clientCert
            //var serverCert = PemImport.Certificate.GetCertificateFromPEMstring(serverCertificatePem);

            //return ConnectAndSecure(server, port, clientCert, serverCert);
            //return null;
        }

        public Stream ConnectAndSecureFromFiles(string server, int port, 
                                                string clientCertificateAndKeyFile,
                                                string serverCertificateAndKey,
                                                int genericTimeoutMs)
        {
            _log.Debug("Loading Cert/Key from {0}", clientCertificateAndKeyFile);
            
            var clientCert = new X509Certificate2(clientCertificateAndKeyFile);
            var serverCert = string.IsNullOrWhiteSpace(serverCertificateAndKey) 
                                    ? null : new X509Certificate2(serverCertificateAndKey);

            return ConnectAndSecure(server, port, clientCert, serverCert, genericTimeoutMs);
        }

        public Stream ConnectAndSecure(string server, int port, X509Certificate2 clientCert, X509Certificate2 serverCert, int genericTimeoutMs = -1)
        {
            _log.Debug("Connecting to {0}:{1}", server, port);
            var tcp = new TcpClient();

            tcp.ReceiveTimeout = genericTimeoutMs;
            tcp.SendTimeout = genericTimeoutMs;

            tcp.Connect(server, port);

            _log.Debug("Establishing secure connection");


            SslStream ret;
#if !XAMARIN_DROID
            if(serverCert != null)
            {
                var remoteMatch = new MatchCertificate(serverCert);
                ret = new SslStream(tcp.GetStream(), false, remoteMatch.OnRemoteCertificateValid, null, EncryptionPolicy.RequireEncryption);
            }
            else
                ret = new SslStream(tcp.GetStream(), false, null, null, EncryptionPolicy.RequireEncryption);
            
#else
            //ServicePointManager.ServerCertificateValidationCallback = remoteMatch.OnRemoteCertificateValid;
            ret = new SslStream(tcp.GetStream(), false, null, null);
#endif
            ret.AuthenticateAsClient(server, new X509Certificate2Collection(clientCert), EnabledSslProtocols,
                CheckCertificateRevocation);

            return ret;
        }

        private class MatchCertificate
        {
            private readonly X509Certificate2 _rootAuthority;
            private readonly ILogger _log = LogManager.GetCurrentClassLogger();

            public MatchCertificate(X509Certificate2 certificate)
            {
                _rootAuthority = certificate;
            }

            public bool OnRemoteCertificateValid(object sender, X509Certificate certificate, X509Chain chain1, SslPolicyErrors sslpolicyerrors)
            {
                var replace = certificate.ToString().Replace("\r", "").Replace("\n", " --- ");

                if (_rootAuthority == null)
                {
                    _log.Warn("accepting all certificates: {0}", replace);
                    return true;
                }
                
                _log.Debug("verifiying server certificate: {0}", replace);
                

                return VerifyCertifcateWithRootCA(new X509Certificate2(certificate), _rootAuthority);
            }

            private bool VerifyCertifcateWithRootCA(X509Certificate2 certificateToValidate, X509Certificate2 authority)
            {
                X509Chain chain = new X509Chain();
                chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
                chain.ChainPolicy.RevocationFlag = X509RevocationFlag.ExcludeRoot;
                chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;
                chain.ChainPolicy.VerificationTime = DateTime.Now;
                chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 0, 0);

                // This part is very important. You're adding your known root here.
                // It doesn't have to be in the computer store at all. Neither certificates do.
                chain.ChainPolicy.ExtraStore.Add(authority);

                bool isChainValid = chain.Build(certificateToValidate);

                if (!isChainValid)
                {
                    string[] errors =
                        chain.ChainStatus.Select(x => String.Format("{0} ({1})", x.StatusInformation.Trim(), x.Status)).ToArray();
                    string certificateErrorsString = "Unknown errors.";

                    if (errors.Length > 0)
                    {
                        certificateErrorsString = String.Join(", ", errors);
                    }

                    throw new Exception("Trust chain did not complete to the known authority anchor. Errors: " +
                                        certificateErrorsString);
                }

                // This piece makes sure it actually matches your known root
                if (chain.ChainElements.Cast<X509ChainElement>().All(x => x.Certificate.Thumbprint != authority.Thumbprint))
                {
                    throw new Exception("Trust chain did not complete to the known authority anchor. Thumbprints did not match.");
                }

                return true;
            }
        }

    }
}
