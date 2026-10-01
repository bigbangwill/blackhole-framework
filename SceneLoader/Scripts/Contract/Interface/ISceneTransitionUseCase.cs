using System.Threading;
using Cysharp.Threading.Tasks;

namespace Blackhole.SceneLoader.Contract
{
    public interface ISceneTransitionUseCase
    {
        UniTask LoadSceneAsync(string sceneName, bool isAdditive, CancellationToken ct);
        UniTask UnloadSceneAsync(string sceneName, CancellationToken ct);
    }
}