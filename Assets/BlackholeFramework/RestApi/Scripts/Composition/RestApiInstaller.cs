using VContainer;
using UnityEngine;
using Blackhole.SharedKernel.Composition;
using Blackhole.RestApi.Contract;
using Blackhole.RestApi.Infrastructure;

namespace Blackhole.RestApi.Installer
{
    [CreateAssetMenu(fileName = "RestApiInstaller", menuName = "Blackhole/Modules/Installers/RestApiInstaller")]
    public class RestApiInstaller : ModuleInstallerSO
    {
        public override void InstallModule(IContainerBuilder builder)
        {
            builder.Register<IRestApiClient, RestApiClient>(Lifetime.Singleton);
        }
    }
}
