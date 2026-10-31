using System;

namespace Blackhole.Networking.Contract
{
    public enum NetworkRole
    {
        Client,
        Server
    }

    /// <summary>Immutable configuration owned by one client or server module session.</summary>
    public sealed class NetworkSessionOptions
    {
        public const ushort DefaultPort = 7788;

        public NetworkRole Role { get; }
        public NetworkAddress Address { get; }

        public NetworkSessionOptions(NetworkRole role, string host, ushort port)
        {
            if (!Enum.IsDefined(typeof(NetworkRole), role))
                throw new ArgumentOutOfRangeException(nameof(role));
            if (string.IsNullOrWhiteSpace(host))
                throw new ArgumentException("A network address is required.", nameof(host));
            if (port == 0)
                throw new ArgumentOutOfRangeException(nameof(port));

            Role = role;
            Address = new NetworkAddress(host, port);
        }
    }
}
