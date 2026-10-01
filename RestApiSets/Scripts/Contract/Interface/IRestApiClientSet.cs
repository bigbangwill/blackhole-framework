using Blackhole.RestApi.Contract;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace Blackhole.RestApiSets.Contract
{
    public interface IRestApiClientSet
    {
        UniTask<RestResult> Send(RestRequest request, CancellationToken ct);
        UniTask<RestResult<T>> Send<T>(RestRequest request, CancellationToken ct);

        UniTask<RestResult> SendAuthenticated(RestRequest request, CancellationToken ct);
        UniTask<RestResult<T>> SendAuthenticated<T>(RestRequest request, CancellationToken ct);
    }
}