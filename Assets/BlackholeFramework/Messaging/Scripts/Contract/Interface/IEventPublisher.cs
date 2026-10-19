namespace Blackhole.Messaging.Contract
{
    public interface IEventPublisher<T>
    {
        public void Publish(T message);
    }
}