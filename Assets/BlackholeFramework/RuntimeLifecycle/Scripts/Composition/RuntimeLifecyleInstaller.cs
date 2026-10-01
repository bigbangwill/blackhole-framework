using VContainer;
using UnityEngine;
using VContainer.Unity;
using Blackhole.SharedKernel.Composition;

namespace Blackhole.RuntimeLifecycle.Infrastructure
{
    [CreateAssetMenu(fileName = "RuntimeLifecyleInstaller", menuName = "Blackhole/Modules/Installers/RuntimeLifecyleInstaller")]
    public class RuntimeLifecyleInstaller : ModuleInstallerSO
    {
        public override void InstallModule(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<LifecycleTickRunner>(Lifetime.Scoped);
            builder.RegisterEntryPoint<LifecyleInitRunner>(Lifetime.Scoped);
        }
    }
}
