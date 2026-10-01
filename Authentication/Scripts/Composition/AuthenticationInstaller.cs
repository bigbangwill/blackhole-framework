using VContainer;
using Blackhole.Authentication.Application;
using Blackhole.Authentication.Infrastructure;
using Blackhole.SharedKernel.Composition;
using UnityEngine;
using Blackhole.Authentication.Contract;

namespace Blackhole.Authentication.Composition
{
    [CreateAssetMenu(fileName = "AuthenticationInstaller", menuName = "Blackhole/Modules/Installers/AuthenticationInstaller")]
    public class AuthenticationInstaller : ModuleInstallerSO
    {
        [SerializeField] private AuthenticationSettingSO authenticationSettingSO;
        [SerializeField] private SessionTokenSO sessionTokenSO;

        public override void InstallModule(IContainerBuilder builder)
        {

            builder.RegisterInstance(RequireAsset(authenticationSettingSO, nameof(authenticationSettingSO)));
            builder.RegisterInstance(RequireAsset(sessionTokenSO, nameof(sessionTokenSO)));
            builder.Register<IAuthenticationGateway, RestAuthenticationGateway>(Lifetime.Singleton);
            builder.Register<IAuthenticatedRestApiClient, AuthenticatedRestApiClient>(Lifetime.Singleton);

            builder.Register<AuthenticationService>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<ISessionTokenDtoRepository, SessionTokenDtoRepository>(Lifetime.Singleton);
        }
    }
}
