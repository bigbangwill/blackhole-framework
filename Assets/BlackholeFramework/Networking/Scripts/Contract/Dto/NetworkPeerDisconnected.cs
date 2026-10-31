namespace Blackhole.Networking.Contract
{
    public readonly struct NetworkPeerDisconnected
    {
        public readonly NetworkPeerId PeerId { get; }
        public NetworkPeerDisconnected(NetworkPeerId peerId)
        {
            PeerId = peerId;
        }
    }
}