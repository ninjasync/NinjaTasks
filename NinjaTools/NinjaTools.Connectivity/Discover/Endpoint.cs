using System;
using System.Net;
using System.Text.RegularExpressions;

namespace NinjaTools.Connectivity.Discover
{
    public enum EndpointType
    {
        Unknown,
        Bluetooth,
        TcpIp,
    }
    // might also be possibly to do something like this with URIs
    public class Endpoint
    {
        public string Name { get; protected set; }
        public string Address { get; protected set; }

        public EndpointType DeviceType { get; protected set; }
        public string Port { get; protected set; }
        public static Endpoint Empty = null;

        public Endpoint(EndpointType type, string name, string address)
        {
            DeviceType = type;
            Name = name;
            Address = address;
        }

        public Endpoint(EndpointType type, string name, string address, string port)
        {
            DeviceType = type;
            Name       = name;
            Address    = address;
            Port       = port;
        }

        public Endpoint WithPort(string port)
        {
            var ret = Clone();
            ret.Port = port;
            return ret;
        }

        public Endpoint WithName(string name)
        {
            var ret = Clone();
            ret.Name = name;
            return ret;
        }

        private Endpoint Clone()
        {
            return new Endpoint(DeviceType, Name, Address, Port);
        }

        public override string ToString()
        {
            string ret = $"{DeviceType.ToString().ToLowerInvariant()}://{Address}";

            if (!string.IsNullOrEmpty(Port))
                ret += "|" + Port;

            if (!string.IsNullOrEmpty(Name) && Name != Address)
                ret = $"{Name.Replace(' ', '_')} ({ret})";

            return ret;
        }

        private static Regex NamedRegex = new Regex(@"^(?<name>[^ ]+) \((?<uri>[^\)]+)\)$");
        private static Regex UriRegex = new Regex(@"^(?<type>[a-zA-Z0-9_]+)://(?<address>[^|]+)(?<port>|.*)?$");

        public static Endpoint FromString(string str)
        {
            if (string.IsNullOrEmpty(str))
                return null;

            string name = null;
            string address = null;
            string port = null;
            EndpointType type;

            var match = NamedRegex.Match(str);
            if (match.Success)
            {
                name = match.Groups["name"].Value;
                str = match.Groups["uri"].Value;
            }

            match = UriRegex.Match(str);
            if(!match.Success)
                throw new ArgumentException("unable to parse:" + str);

            address = match.Groups["address"].Value;
            port = match.Groups["port"].Success ? match.Groups["port"].Value : null;

            type = (EndpointType)Enum.Parse(typeof(EndpointType), match.Groups["type"].Value, true);

            return new Endpoint(type, name ?? address, address, port);
        }

        #region Equals
        protected bool Equals(Endpoint other)
        {
            return Name == other.Name && Address == other.Address && DeviceType == other.DeviceType && Port == other.Port;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Endpoint) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Address != null ? Address.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int) DeviceType;
                hashCode = (hashCode * 397) ^ (Port != null ? Port.GetHashCode() : 0);
                return hashCode;
            }
        }
        #endregion

        public static Endpoint IpTarget(string address, int port)
        {
            return  new Endpoint(EndpointType.TcpIp, address, address, port.ToString());
        }
    }
}