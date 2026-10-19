using Blackhole.Messaging.Contract;
using MessagePipe;
using System;

namespace Blackhole.Messaging.Infrastructure
{
    public class MessagePipeEventSubscriber<T> : IEventSubscriber<T>
    {
        private readonly ISubscriber<T> _subscriber;

        public MessagePipeEventSubscriber(ISubscriber<T> subscriber)
        {
            _subscriber = subscriber;
        }

        public IDisposable Subscribe(Action<T> handler)
        {
            return _subscriber.Subscribe(handler);
        }
    }
}