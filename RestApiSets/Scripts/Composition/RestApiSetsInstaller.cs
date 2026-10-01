using VContainer;
using Blackhole.SharedKernel.Composition;
using UnityEngine;
using Blackhole.RestApiSets.Infrastructure;
using Blackhole.RestApiSets.Contract;
using Blackhole.Authentication.Contract;
using Blackhole.RestApi.Contract;

namespace Blackhole.RestApiSets.Composition
{
    [CreateAssetMenu(fileName = "RestApiSetsInstaller", menuName = "Blackhole/Modules/Installers/RestApiSetsInstaller")]
    public class RestApiSetsInstaller : ModuleInstallerSO
    {
        [SerializeField] private RestApiSetSettingSO restApiSetSettingSO;

        public override void InstallModule(IContainerBuilder builder)
        {
            RequireAsset(restApiSetSettingSO, nameof(restApiSetSettingSO));

            foreach (var urlInfo in restApiSetSettingSO.Urls)
            {
                var server = urlInfo.Server;
                string baseUrl = urlInfo.Url;

                builder.Register<IRestApiClientSet>(resolver =>
                {
                    IAuthenticatedRestApiClient authenticatedRestApiClient = resolver.Resolve<IAuthenticatedRestApiClient>();
                    IRestApiClient restApiClient = resolver.Resolve<IRestApiClient>();

                    return new RestApiClientSet(baseUrl, authenticatedRestApiClient, restApiClient);
                }, Lifetime.Singleton).Keyed(server);
            }
        }
    }
}
