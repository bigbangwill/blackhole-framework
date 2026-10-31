using Blackhole.Networking.Contract;

namespace Blackhole.Networking.Application
{
    public interface INetworkPayloadHandler
    {
        void Handle(NetworkPeerId id, NetworkPayload payload);
    }
}