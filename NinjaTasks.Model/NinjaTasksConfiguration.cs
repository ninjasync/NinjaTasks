using System.ComponentModel;
using NinjaTools;

namespace NinjaTasks.Model
{
    public interface INinjaTasksConfigurationService : IConfigurationService<NinjaTasksConfiguration>
    {
    }

    public class NinjaTasksConfiguration : INotifyPropertyChanged
    {
        public const int DefaultTcpIpPort = 5665;

        public bool RunBluetoothServer { get; set; }
        public bool RunTcpIpServer { get; set; }
        public int TcpIpServerPort { get; set; }

        public bool ShowCompletedTasks { get; set; }

        public NinjaTasksConfiguration()
        {
            TcpIpServerPort = DefaultTcpIpPort;
        }


        #pragma warning disable CS0067
        public event PropertyChangedEventHandler PropertyChanged;
        #pragma warning restore CS0067
    }


}
