using Blackhole.ModuleSystem.Contract;
using UnityEngine;
using Blackhole.SharedKernel.Composition;
using VContainer;

namespace Blackhole.ModuleSystem.Composition
{
    [CreateAssetMenu(fileName = "ModuleSystemInstaller", menuName = "Blackhole/Modules/Installers/ModuleSystemInstaller")]
    public sealed class ModuleSystemInstaller : ModuleInstallerSO
    {
        public override void InstallModule(IContainerBuilder builder)
        {
            RegisterServices(builder);
        }

        public static void RegisterServices(IContainerBuilder builder)
        {
            builder.Register<IModuleLoader, RuntimeModuleLoader>(Lifetime.Singleton);
            builder.Register<ModuleResolver>(Lifetime.Singleton).AsSelf();
        }
    }
}
