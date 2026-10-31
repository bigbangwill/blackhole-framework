namespace Blackhole.Networking.Contract
{
    public readonly struct NetworkAddress
    {
        public readonly string Host;
        public readonly ushort Port;

        public NetworkAddress(string host, ushort port)
        {
            Host = host;
            Port = port;
        }
    }
}