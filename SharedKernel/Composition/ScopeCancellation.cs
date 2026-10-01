using System;
using System.Threading;
using VContainer;

namespace Blackhole.SharedKernel.Composition
{
    public sealed class ScopeCancellation : IDisposable
    {
        private readonly CancellationTokenSource source = new();
        private bool disposed;

        public CancellationToken Token => source.Token;

        public void Cancel()
        {
            if (!disposed && !source.IsCancellationRequested)
            {
                source.Cancel();
            }
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;

            try
            {
                source.Cancel();
            }
            finally
            {
                source.Dispose();
            }
        }
    }

    public static class ScopeCancellationInstaller
    {
        public static void Install(IContainerBuilder builder)
        {
            builder.Register<ScopeCancellation>(Lifetime.Singleton);
            builder.Register<CancellationToken>(
                resolver => resolver.Resolve<ScopeCancellation>().Token,
                Lifetime.Singleton);
        }
    }
}
