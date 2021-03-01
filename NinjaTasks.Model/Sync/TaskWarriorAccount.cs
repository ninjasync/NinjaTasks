using System.ComponentModel;
using NinjaTools.Sqlite;

namespace NinjaTasks.Model.Sync
{
    public class TaskWarriorAccount : INotifyPropertyChanged
    {
        [PrimaryKey]
        public int Id { get; set; }

        public string ServerHostname { get; set; }
        public int ServerPort { get; set; }

        public string Org { get; set; }
        public string User { get; set; }
        public string Key { get; set; }

        /// <summary>
        /// this is a string in PEM format
        /// </summary>
        public string ClientCertificateAndKeyPem { get; set; }
        public string ClientCertificateAndKeyPfxFile { get; set; }
        /// <summary>
        /// this is a string in PEM format
        /// </summary>
        public string ServerCertificatePem { get; set; }
        public string ServerCertificateCrtFile { get; set; }

        [Ignore]
        public bool IsValid
        {
            get 
            { 
                return !string.IsNullOrWhiteSpace(ServerHostname) 
                        && ServerPort!=0 
                        && !string.IsNullOrEmpty(Org) 
                        && !string.IsNullOrEmpty(Key)
                        && (!string.IsNullOrEmpty(ClientCertificateAndKeyPfxFile) || !string.IsNullOrEmpty(ClientCertificateAndKeyPem));
            }
        }

        [Ignore]
        public bool IsEmpty
        {
            get
            {
                return string.IsNullOrWhiteSpace(ServerHostname)
                                        && ServerPort == 0
                                        && string.IsNullOrEmpty(Org)
                                        && string.IsNullOrEmpty(Key)
                                        && string.IsNullOrEmpty(ClientCertificateAndKeyPfxFile)
                                        && string.IsNullOrEmpty(ServerCertificateCrtFile)
                                        && string.IsNullOrEmpty(ClientCertificateAndKeyPem)
                                        && string.IsNullOrEmpty(ServerCertificatePem)
                                        ;
            }
        }

        #pragma warning disable CS0067
        public event PropertyChangedEventHandler PropertyChanged;
        #pragma warning restore CS0067
    }
}
