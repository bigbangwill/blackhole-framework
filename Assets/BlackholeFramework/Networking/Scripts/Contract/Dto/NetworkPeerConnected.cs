namespace Blackhole.Networking.Contract
{
    public readonly struct NetworkPeerConnected
    {
        public readonly NetworkPeerId PeerId { get; }

        public NetworkPeerConnected(NetworkPeerId peerId)
        {
            PeerId = peerId;
        }
    }
}