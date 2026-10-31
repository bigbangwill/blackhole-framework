namespace Blackhole.Networking.Contract
{
    public interface INetworkServer
    {
        public bool TrySend(NetworkPeerId peerId, NetworkPayload payload);
        public bool TryBroadcast(NetworkPayload payload);
    }
}