using System.Threading;
using Cysharp.Threading.Tasks;

namespace Blackhole.RestApi.Contract
{
    public interface IRestApiClient
    {
        UniTask<RestResult> Send(RestRequest request, CancellationToken ct);
        UniTask<RestResult<T>> Send<T>(RestRequest request, CancellationToken ct);
    }
}