using Blackhole.SharedKernel.Composition;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Blackhole.Framework.Composition
{
    [CreateAssetMenu(fileName = "FrameworkInstaller", menuName = "Blackhole/Modules/Installers/FrameworkInstaller")]
    public sealed class FrameworkInstaller : ModuleInstallerSO
    {
        public override void InstallModule(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<FrameworkEntryPoint>();
        }
    }
}
