
namespace TaskWarriorLib
{
    public class TaskWarriorAccount 
    {
        public string ServerHostname { get; set; }
        public int ServerPort { get; set; }

        public string Org { get; set; }
        public string User { get; set; }
        public string Key { get; set; }

        public string ClientCertificateAndKeyPem { get; set; }
        public string ServerCertificatePem { get; set; }

        public string ClientCertificateAndKeyPfxFile { get; set; }
        public string ServerCertificateCrtFile { get; set; }
    }
}
