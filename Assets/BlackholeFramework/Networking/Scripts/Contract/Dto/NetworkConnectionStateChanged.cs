namespace Blackhole.Networking.Contract
{
    public readonly struct NetworkConnectionStateChanged
    {
        public readonly NetworkConnectionState Previous;
        public readonly NetworkConnectionState Current;

        public NetworkConnectionStateChanged(NetworkConnectionState previous, NetworkConnectionState current)
        {
            Previous = previous;
            Current = current;
        }
    }
}