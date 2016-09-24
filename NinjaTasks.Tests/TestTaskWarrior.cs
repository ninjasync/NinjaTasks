using System.Collections.Generic;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Newtonsoft.Json;
using NinjaTasks.App.Wpf.Services;
using NinjaTasks.App.Wpf.Services.TcpIp;
using NinjaTasks.Sync.TaskWarrior;
using NinjaTools.Progress;
using NUnit.Framework;
using TaskWarriorLib;
using TaskWarriorLib.Parser;

namespace NinjaTasks.Tests
{
    [TestFixture]
    public class TestTaskWarrior
    {
        const int Port = 8020;
        const string Hostname = "knutwg";

        public static readonly TaskWarriorAccount Account = new TaskWarriorAccount
        {
            ClientCertificateAndKeyPfxFile = @"n:\xdata\certificates\PublicOlaf-taskd.pfx",
            Org = "Public",
            User = "Olaf",
            Key = "145d91a2-37f9-41b4-b7c0-cfcb6080101e",
            ServerHostname = Hostname,
            ServerPort = Port
        };

        [Test]
        public void TestTcpConnection()
        {
            TcpClient tcp = new TcpClient();
            tcp.Connect(Hostname, Port);
            Assert.IsTrue(tcp.Connected);
            tcp.Close();
        }

        [Test]
        public void TestTLSConnection()
        {
            using (TcpClient tcp = new TcpClient(Hostname, Port))
            {
                Assert.IsTrue(tcp.Connected);

                var clientCert = new X509Certificate2(Account.ClientCertificateAndKeyPfxFile);
                Assert.IsTrue(clientCert.HasPrivateKey);
                X509Certificate2Collection certCol = new X509Certificate2Collection(clientCert);

                using(var ssl = new SslStream(tcp.GetStream(), false, OnRemoteCertificateValid, null, EncryptionPolicy.RequireEncryption))
                {
                    ssl.AuthenticateAsClient(Hostname, certCol, SslProtocols.Default, true);
                    Assert.IsTrue(ssl.IsAuthenticated);
                    Assert.IsTrue(ssl.IsEncrypted);
                    Assert.IsTrue(!ssl.IsServer);
                    Assert.IsTrue(ssl.IsSigned);
                    Assert.IsTrue(ssl.IsMutuallyAuthenticated);
                }
            }
        }

        [Test]
        public void TestSyncRequest()
        {
            var dx = new TaskWarriorSyncDataExchange(new TslConnectionFactory());
            SyncBundle local = new SyncBundle();
            
            SyncBundle remote = dx.ExchangeSyncData(Account, local, new NullProgress());

            Assert.IsNotNull(remote.SyncId);
            Assert.Greater(remote.ChangedTasks.Count, 1);
        }


        [TestCase("{\"description\":\"test3\",\"entry\":\"20150129T171735Z\",\"modified\":\"20150129T171735Z\",\"project\":\"test\",\"status\":\"pending\",\"uuid\":\"c681af5a-666a-4479-a7de-20cabd251c45\"}")]
        [TestCase("{\"description\":\"meine erste aufgabe\",\"end\":\"20150128T003021Z\",\"entry\":\"20150128T001630Z\",\"modified\":\"20150128T002957Z\",\"progress\":\"100.000000\",\"project\":\"Eingang\",\"status\":\"completed\",\"uuid\":\"ce814eaf-3ca5-46c6-846a-1f522ad1b80a\",\"XXunknown\":\"0\"}")]
        public void TestJsonParse(string json)
        {
            var settings = TaskWarriorTaskParser.JsonSettings;

            var task = JsonConvert.DeserializeObject<TaskWarriorTask>(json, settings);
            Assert.IsTrue(task.IsValid);

            if (json.Contains("XXun"))
                Assert.IsNotNull(task.AdditionalData);

            var repr = JsonConvert.SerializeObject(task, settings);
            Assert.IsTrue(repr.StartsWith("{"));

            if (json.Contains("XXun"))
                Assert.IsTrue(repr.Contains("XXun"));
        }

        private bool OnRemoteCertificateValid(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslpolicyerrors)
        {
            return true;
        }

        [Test]
        public void TestParseTaskd()
        {
            var taskd = TaskdConfigFile.Parse(File.ReadAllText(@"Olaf.taskdconfig", Encoding.UTF8));
            //var client = PemImport1.Certificate.GetCertificateWithKeyFromPEMstring(taskd.ClientCertificateAndKey, "");
            //var rootca = PemImport1.Certificate.GetCertificateFromPEMstring(taskd.RootCaCertificate);

            //Assert.AreEqual("CN=Göteborg Bit Factory, O=Göteborg Bit Factory", client.SubjectName.Name);
            //Assert.IsNotNull(client.PrivateKey);
            //Assert.IsTrue(client.HasPrivateKey);
            //Assert.AreEqual("knutwg", rootca.SubjectName.Name);

            //var client = PemImport1.FromPem(taskd.ClientCertificateAndKey);
        }
        
    }
}
