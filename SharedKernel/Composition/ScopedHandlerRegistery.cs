using Blackhole.SharedKernel.Contract;
using System;

namespace Blackhole.SharedKernel.Composition
{
    public class ScopedHandlerRegistery<THandler> : IScopedHandlerRegistery<THandler>
    {
        private THandler _handler;

        public void Register(THandler handler)
        {
            if (_handler != null)
                throw new InvalidOperationException($"A handler for {typeof(THandler).Name} is already registered.");

            _handler = handler;
        }

        public void Unregister(THandler handler)
        {
            if (ReferenceEquals(_handler, handler))
            {
                _handler = default;
            }
        }

        public bool TryGet(out THandler handler)
        {
            handler = _handler;
            return handler != null;
        }
    }
}