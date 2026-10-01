using Blackhole.RestApi.Contract;
using Blackhole.RestApiSets.Contract;
using Blackhole.Authentication.Contract;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace Blackhole.RestApiSets.Infrastructure
{
    public class RestApiClientSet : IRestApiClientSet
    {
        private readonly string _baseUrl;

        private readonly IRestApiClient _restApiClient;
        private readonly IAuthenticatedRestApiClient _authenticatedApiClient;

        public RestApiClientSet(string baseUrl, IAuthenticatedRestApiClient authenticatedApiClient, IRestApiClient restApiClient)
        {
            _baseUrl = baseUrl;
            _authenticatedApiClient = authenticatedApiClient;
            _restApiClient = restApiClient;
        }

        public async UniTask<RestResult> Send(RestRequest request, CancellationToken ct)
        {
            request.SetBaseUrl(_baseUrl);
            return await _restApiClient.Send(request, ct);
        }

        public async UniTask<RestResult<T>> Send<T>(RestRequest request, CancellationToken ct)
        {
            request.SetBaseUrl(_baseUrl);
            return await _restApiClient.Send<T>(request, ct);
        }

        public async UniTask<RestResult> SendAuthenticated(RestRequest request, CancellationToken ct)
        {
            request.SetBaseUrl(_baseUrl);
            return await _authenticatedApiClient.Send(request, ct);
        }

        public async UniTask<RestResult<T>> SendAuthenticated<T>(RestRequest request, CancellationToken ct)
        {
            request.SetBaseUrl(_baseUrl);
            return await _authenticatedApiClient.Send<T>(request, ct);
        }
    }
}