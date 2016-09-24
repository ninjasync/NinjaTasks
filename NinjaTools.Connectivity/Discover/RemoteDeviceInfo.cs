namespace NinjaTools.Connectivity.Discover
{
    public enum RemoteDeviceInfoType
    {
        Unknown,
        Bluetooth,
        TcpIp,
    }
    public class RemoteDeviceInfo
    {
        public string Name { get; protected set; }
        public string Address { get; protected set; }

        public RemoteDeviceInfoType DeviceType { get; protected set; }
        public string Port { get; set; }

        public RemoteDeviceInfo(RemoteDeviceInfoType type, string name, string address)
        {
            DeviceType = type;
            Name = name;
            Address = address;
        }
    }
}