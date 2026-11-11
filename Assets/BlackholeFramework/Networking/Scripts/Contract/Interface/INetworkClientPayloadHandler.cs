namespace Blackhole.Networking.Contract
{
    public interface INetworkClientPayloadHandler
    {
        public void Handle(NetworkPayload payload);
    }
}