using Blackhole.SharedKernel.Contract;

namespace Blackhole.Networking.Contract
{
    public interface INetworkClient
    {
        public NetworkConnectionState State { get; }

        Result Send(NetworkPayload payload);

        Result Connect(NetworkAddress address);
        Result Disconnect();
    }
}
