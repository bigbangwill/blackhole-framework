using Blackhole.Networking.Contract;

namespace Blackhole.Networking.Application
{
    public interface INetworkClientPayloadHandler
    {
        public void Handle(NetworkPayload payload);
    }
}