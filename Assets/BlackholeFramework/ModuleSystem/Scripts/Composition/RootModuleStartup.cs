using Blackhole.ModuleSystem.Contract;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using VContainer.Unity;

namespace Blackhole.ModuleSystem.Composition
{
    public abstract class RootModuleStartup : IRootModuleStartup, IInitializable, IDisposable
    {
        private readonly RootModuleStartupRegistry registry;

        protected RootModuleStartup(RootModuleStartupRegistry registry)
        {
            this.registry = registry ?? throw new ArgumentNullException(nameof(registry));
        }

        public virtual UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            return UniTask.CompletedTask;
        }

        public void Initialize()
        {
            registry.Register(this);
        }

        public void Dispose()
        {
            registry.Unregister(this);
        }
    }

    public sealed class RootModuleStartupRegistry
    {
        private IRootModuleStartup startup;

        public void Register(IRootModuleStartup rootModuleStartup)
        {
            if (rootModuleStartup == null)
                throw new ArgumentNullException(nameof(rootModuleStartup));

            if (startup == null)
            {
                startup = rootModuleStartup;
                return;
            }

            if (ReferenceEquals(startup, rootModuleStartup))
                return;

            throw new InvalidOperationException("More than one root startup was registered in the module session.");
        }

        public void Unregister(IRootModuleStartup rootModuleStartup)
        {
            if (ReferenceEquals(startup, rootModuleStartup))
                startup = null;
        }

        public IRootModuleStartup GetRequired(string moduleId)
        {
            if (startup != null)
                return startup;

            throw new InvalidOperationException(
                $"Root module '{moduleId}' loaded its scenes but did not register an {nameof(IRootModuleStartup)}. " +
                $"Register a {nameof(RootModuleStartup)} entry point in the root scene's LifetimeScope.");
        }
    }
}
