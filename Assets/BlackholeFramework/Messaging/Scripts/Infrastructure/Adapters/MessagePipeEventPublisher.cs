using Blackhole.Messaging.Contract;
using MessagePipe;

namespace Blackhole.Messaging.Infrastructure
{
	public class MessagePipeEventPublisher<T> : IEventPublisher<T>
    {
	    private readonly IPublisher<T> _publisher;

	    public MessagePipeEventPublisher(IPublisher<T> publisher)
	    {
		    _publisher = publisher;
	    }

        public void Publish(T message)
		{
			_publisher.Publish(message);
		}
    }
}