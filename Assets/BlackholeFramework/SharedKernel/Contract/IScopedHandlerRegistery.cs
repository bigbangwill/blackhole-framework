namespace Blackhole.SharedKernel.Contract
{
    public interface IScopedHandlerRegistery<THandler>
    {
        void Register(THandler handler);
        void Unregister(THandler handler);
        bool TryGet(out  THandler handler);
    }
}