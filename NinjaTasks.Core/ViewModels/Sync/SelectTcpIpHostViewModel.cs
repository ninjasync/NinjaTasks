using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using Cirrious.MvvmCross.Plugins.Messenger;
using NinjaTools;
using NinjaTools.Connectivity.Discover;
using NinjaTools.Connectivity.ViewModels.Messages;
using NinjaTools.MVVM;

namespace NinjaTasks.Core.ViewModels.Sync
{
    public class SelectTcpIpHostViewModel : BaseViewModel
    {
        private readonly IMvxMessenger _messenger;
        private string _host;

        public string Host 
        {
            get
            { 
                return _host; 
            }
            set
            {
                if (value != null && value.Contains(":") && !value.EndsWith(":"))
                    HostAndPort = value;
                else
                    _host = value;
            } 
        }


        public int Port { get; set; }

        public string Id { get; private set; }

        public SelectTcpIpHostViewModel(IMvxMessenger messenger)
        {
            _messenger = messenger;
#if !DOT42
            AddToAutoBundling(()=> Id);
            AddToAutoBundling(() => Host);
            AddToAutoBundling(() => Port);
#else
            AddToAutoBundling("Id");
            AddToAutoBundling("Host");
            AddToAutoBundling("Port");
#endif
        }

        public string HostAndPort
        {
            get { return Host + ":" + ((Port==0)?"":Port.ToStringInvariant()); }
            set
            {
                var v = SplitHostAndPort(value);
                Host = v.Item1;
                Port = v.Item2;
            }
        }

        public bool CanSelect { get { return Port != 0 && !Host.IsNullOrWhiteSpace() && !Host.EndsWith(":"); } }
        public void Select()
        {
            _messenger.Publish(new RemoteDeviceSelectedMessage(this, Id, new RemoteDeviceInfo(RemoteDeviceInfoType.TcpIp, Host, Host)
            {
                Port = Port.ToStringInvariant()
            }));
            Close(this);
        }

        public static Tuple<string, int> SplitHostAndPort(string hostAndPort)
        {
            string host = "";
            int port=0;

            if (hostAndPort.IsNullOrEmpty())
                return Tuple.Create(host, port);

            int idx = hostAndPort.IndexOf(':');
            if (idx == -1)
                host = hostAndPort.Trim();
            else
            {
                host = hostAndPort.Substring(0, idx);
                
                StringBuilder portstr = new StringBuilder();
                foreach(char c in hostAndPort.Substring(idx + 1))
                    if (char.IsDigit(c))
                        portstr.Append(c);
                if (portstr.Length == 0)
                    port = 0;
                else
                    port = int.Parse(portstr.ToString(), CultureInfo.InvariantCulture);
            }

            return Tuple.Create(host, port);
        }
    }
}
