namespace Blackhole.Networking.Contract
{
    public interface INetworkPayloadHandler
    {
        void Handle(NetworkPeerId id, NetworkPayload payload);
    }
}