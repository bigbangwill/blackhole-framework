using System;

namespace Blackhole.Messaging.Contract
{
    public interface IEventSubscriber<T>
    {
        public IDisposable Subscribe(Action<T> handler);
    }
}