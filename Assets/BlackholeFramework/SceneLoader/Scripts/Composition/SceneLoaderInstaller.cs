using Blackhole.SceneLoader.Application;
using UnityEngine;
using Blackhole.SceneLoader.Contract;
using Blackhole.SceneLoader.Infrastructure;
using Blackhole.SharedKernel.Composition;
using VContainer;

namespace Blackhole.SceneLoader.Composition
{
    [CreateAssetMenu(fileName = "SceneLoaderInstaller", menuName = "Blackhole/Modules/Installers/SceneLoaderInstaller")]
    public sealed class SceneLoaderInstaller : ModuleInstallerSO
    {
        public override void InstallModule(IContainerBuilder builder)
        {
            builder.Register<ISceneLoaderPort, UnitySceneLoaderPort>(Lifetime.Singleton);
            builder.Register<ISceneTransitionUseCase, SceneTransitionUseCase>(Lifetime.Singleton);
        }
    }
}
