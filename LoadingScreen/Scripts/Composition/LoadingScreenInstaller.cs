using Blackhole.LoadingScreen.Application;
using UnityEngine;
using Blackhole.LoadingScreen.Contract;
using Blackhole.SharedKernel.Composition;
using VContainer;

namespace Blackhole.LoadingScreen.Composition
{
    [CreateAssetMenu(fileName = "LoadingScreenInstaller", menuName = "Blackhole/Modules/Installers/LoadingScreenInstaller")]
    public class LoadingScreenInstaller : ModuleInstallerSO
    {
        public override void InstallModule(IContainerBuilder builder)
        {
            builder.Register<LoadingScreenPresenterRegistry>(Lifetime.Singleton);
            builder.Register<ILoadingScreenService, LoadingScreenUseCase>(Lifetime.Singleton);
        }
    }
}
