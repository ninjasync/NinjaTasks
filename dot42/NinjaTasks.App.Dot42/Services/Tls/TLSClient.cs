using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Android.Util;
using Java.Net;
using Java.Security;
using Java.Security.Cert;
using Java.Security.Interfaces;
using Java.Security.Spec;
using Java.Text;
using Javax.Net.Ssl;
using NinjaTools.Connectivity.Streams;
using NinjaTools.Logging;
using Exception = System.Exception;
using String = System.String;

namespace NinjaTasks.App.Droid.Services.Tls
{
    public class TLSClient : IDisposable
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();


        public class NoSuchCertificateException : Exception
        {
        }

        private static List<X509Certificate> GenerateCertificateFromPEM(string cert)
        {
            if (cert == null)
                throw new NoSuchCertificateException();

            var parts = ExtractFromPem(cert, "-----BEGIN CERTIFICATE-----", "-----END CERTIFICATE-----").ToList();
            var certs = new List<X509Certificate>();

            foreach (string part in parts)
            {
                try
                {
                    certs.Add(
                        (X509Certificate)
                            CertificateFactory.GetInstance("X.509")
                                .GenerateCertificate(new MemoryStream(Encoding.UTF8.GetBytes(part))));
                }
                catch (CertificateException e)
                {
                    Log.Error("parsing failed:" + part);
                    Log.Error(e);
                    return certs;
                }
            }
            return certs;
        }

        private static IRSAPrivateKey GeneratePrivateKeyFromPem(string key)
        {
            byte[] keyBytes = ParseDerFromPem(key, "-----BEGIN RSA PRIVATE KEY-----", "-----END RSA PRIVATE KEY-----");
            PKCS8EncodedKeySpec spec = new PKCS8EncodedKeySpec(keyBytes);
            KeyFactory factory;

            try
            {
                factory = KeyFactory.GetInstance("RSA", "BC");
            }
            catch (NoSuchAlgorithmException e)
            {
                Log.Error("RSA-Algorithm not found: {0}", e);
                return null;
            }
            catch (NoSuchProviderException e)
            {
                Log.Error("BC not found", e);
                return null;
            }

            try
            {
                var privateKey = factory.GeneratePrivate(spec);
                return (IRSAPrivateKey)privateKey;
            }
            catch (InvalidKeySpecException e)
            {
                Log.Error("cannot parse key", e);
                return null;
            }
        }

        private static byte[] ParseDerFromPem(string pem, string beginDelimiter, string endDelimiter)
        {
            var extract = ExtractFromPem(pem, beginDelimiter, endDelimiter).FirstOrDefault();
            if (extract == null)
                throw new ParseException("Wrong PEM format", 0);

            // remove delimiters.
            extract = extract.Substring(beginDelimiter.Length, extract.Length - beginDelimiter.Length - endDelimiter.Length)
                             .Trim();
            return Base64.Decode(extract, Base64.NO_PADDING);
        }

        private static IEnumerable<string> ExtractFromPem(String pem, String beginDelimiter, String endDelimiter)
        {
            int idxBegin = 0, idxEnd = -1;
            while (idxBegin < pem.Length)
            {
                idxBegin = pem.IndexOf(beginDelimiter, idxEnd + 1, StringComparison.Ordinal);
                idxEnd = pem.IndexOf(endDelimiter, idxBegin + beginDelimiter.Length, StringComparison.Ordinal);

                if (idxBegin == -1 || idxEnd == -1)
                    yield break;
                yield return pem.Substring(idxBegin, idxEnd - idxBegin + endDelimiter.Length);
            }
        }

        private SSLSocket _socket;

        private Stream input;
        private Stream output;

        private SSLSocketFactory sslFact;

        public Stream Stream { get; private set; }

        // //////////////////////////////////////////////////////////////////////////////
        public TLSClient()
        {
            this._socket = null;
            this.sslFact = null;
            this.input = null;
            this.output = null;
        }

        public void Dispose()
        {
            Close();
        }

        public void Close()
        {
            if (this._socket == null)
            {
                Log.Error("socket null");
                return;
            }
            try
            {
                Stream.Flush();
                Stream.Close();
                Stream = null;
                //this.output.Flush();
                //this.input.Close();
                //this.output.Close();
                this._socket.Close();
                this._socket = null;

            }
            catch (Exception e)
            {
                Log.Error("Cannot close Socket: {0}", e);
            }
        }

        private static void SetReasonableEncryption(SSLSocket ssl)
        {
            // set reasonable SSL/TLS settings before the handshake:

            // - enable all supported protocols (enables TLSv1.1 and TLSv1.2 on Android <4.4.3, if available)
            // - remove all SSL versions (especially SSLv3) because they're insecure now
            List<string> protocols = new List<string>();
            foreach (string protocol in ssl.SupportedProtocols)
            {
                if (!protocol.ToUpper().Contains("SSL"))
                    protocols.Add(protocol);
            }

            //Log.v(TAG, "Setting allowed TLS protocols: " + TextUtils.join(", ", protocols));
            ssl.EnabledProtocols = protocols.ToArray();

            // choose secure cipher suites
            string[] allowedCiphers = new[]
            {
                // allowed secure ciphers according to NIST.SP.800-52r1.pdf Section 3.3.1 (see docs directory)
                // TLS 1.2
                "TLS_ECDHE_ECDSA_WITH_AES_256_GCM_SHA384", "TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256",
                "TLS_ECHDE_RSA_WITH_AES_128_GCM_SHA256", "TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA256",
                "TLS_RSA_WITH_AES_256_GCM_SHA384", "TLS_RSA_WITH_AES_128_GCM_SHA256",
                "TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA256",

                // maximum interoperability
                "TLS_RSA_WITH_3DES_EDE_CBC_SHA", "TLS_RSA_WITH_AES_128_CBC_SHA",
                // additionally
                "TLS_RSA_WITH_AES_256_CBC_SHA", "TLS_ECDHE_ECDSA_WITH_3DES_EDE_CBC_SHA",
                "TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA", "TLS_ECDHE_RSA_WITH_3DES_EDE_CBC_SHA",
                "TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA"
            };

            string[] availableCiphers = ssl.SupportedCipherSuites;

            // preferred ciphers = allowed Ciphers \ availableCiphers
            HashSet<String> preferredCiphers = new HashSet<string>(allowedCiphers.Intersect(availableCiphers));
            // add preferred ciphers to enabled ciphers
            // for maximum security, preferred ciphers should *replace* enabled ciphers,
            // but I guess for the security level of DAVdroid, disabling of insecure
            // ciphers should be a server-side task
            HashSet<string> enabledCiphers = new HashSet<string>(ssl.EnabledCipherSuites);
            enabledCiphers.UnionWith(preferredCiphers);

            //Log.v(TAG, "Setting allowed TLS ciphers: " + TextUtils.join( ", ", enabledCiphers));
            if (preferredCiphers.Count == 0)
                ssl.EnabledCipherSuites = enabledCiphers.ToArray();
            else
                ssl.EnabledCipherSuites = preferredCiphers.ToArray();

        }

        // //////////////////////////////////////////////////////////////////////////////
        public Stream Connect(string host, int port)
        {
            Log.Info("connect");
            if (_socket != null)
            {
                try
                {
                    _socket.Close();
                }
                catch (IOException e)
                {
                    Log.Error("cannot close socket {0}", e);
                }
            }
            try
            {
                Log.Debug("connecting to {0}:{1}", host, port);
                this._socket = (SSLSocket) this.sslFact.CreateSocket();
                SetReasonableEncryption(this._socket);
                this._socket.UseClientMode = true;
                this._socket.EnableSessionCreation = true;
                this._socket.NeedClientAuth = true;
                this._socket.TcpNoDelay = true;
                this._socket.Connect(new InetSocketAddress(host, port));
                this._socket.StartHandshake();
                this.output = new JavaOutputStreamWrapper(this._socket.OutputStream);
                this.input = new JavaInputStreamWrapper(this._socket.InputStream);

                Stream = new CombinedStream(input, output, this);

                Log.Debug("connected to {0}:{1}", host, port);
                return Stream;
            }
            catch (UnknownHostException e)
            {
                Log.Error("Unknown Host: {0}", e);
                throw;
            }
            catch (ConnectException e)
            {
                Log.Error("Cannot connect to Host {0}", e);
                throw;
            }
            catch (SocketException e)
            {
                Log.Error("IO Error {0}", e);
                throw;
            }
        }

        // //////////////////////////////////////////////////////////////////////////////
        public void Init(string rootCaCertPem, string userCertPem, string userKeyPem)
        {
            try
            {
                List<X509Certificate> ROOT = GenerateCertificateFromPEM(rootCaCertPem);
                X509Certificate USER_CERT = GenerateCertificateFromPEM(userCertPem).First();
                IRSAPrivateKey USER_KEY = GeneratePrivateKeyFromPem(userKeyPem);

                KeyStore trusted = KeyStore.GetInstance(KeyStore.DefaultType);

                trusted.Load(null);
                Certificate[] chain = new Certificate[ROOT.Count + 1];

                int i = chain.Length - 1;
                foreach (X509Certificate cert in ROOT)
                {
                    trusted.SetCertificateEntry("taskwarrior-ROOT", cert);
                    chain[i--] = cert;
                }
                trusted.SetCertificateEntry("taskwarrior-USER", USER_CERT);
                chain[0] = USER_CERT;

                KeyManagerFactory keyManagerFactory = KeyManagerFactory.GetInstance(KeyManagerFactory.DefaultAlgorithm);
                // Hack to get it working on android 2.2
                String pwd = "secret";
                trusted.SetEntry("user", new KeyStore.PrivateKeyEntry(USER_KEY, chain),
                    new KeyStore.PasswordProtection(pwd.ToCharArray()));
                keyManagerFactory.Init(trusted, pwd.ToCharArray());
                SSLContext context = SSLContext.GetInstance("TLS");
                TrustManagerFactory tmf = TrustManagerFactory.GetInstance(TrustManagerFactory.DefaultAlgorithm);
                tmf.Init(trusted);
                ITrustManager[] trustManagers = tmf.TrustManagers;
                context.Init(keyManagerFactory.KeyManagers, trustManagers, new SecureRandom());
                this.sslFact = context.SocketFactory;

            }
            catch (UnrecoverableKeyException e)
            {
                Log.Warn("cannot restore key");
                throw new CertificateException(e);
            }
            catch (KeyManagementException e)
            {
                Log.Warn("cannot access key");
                throw new CertificateException(e);
            }
            catch (KeyStoreException e)
            {
                Log.Warn("cannot handle keystore");
                throw new CertificateException(e);
            }
            catch (NoSuchAlgorithmException e)
            {
                Log.Warn("no matching algorithm found");
                throw new CertificateException(e);
            }
            catch (CertificateException e)
            {
                Log.Warn("certificat not readable");
                throw new CertificateException(e);
            }
            catch (IOException e)
            {
                Log.Warn("general io problem");
                throw new CertificateException(e);
            }
        }
    }
}
