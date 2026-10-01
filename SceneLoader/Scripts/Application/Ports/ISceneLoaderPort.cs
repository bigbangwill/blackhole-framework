using System.Threading;
using Cysharp.Threading.Tasks;

namespace Blackhole.SceneLoader.Application
{
    public interface ISceneLoaderPort
    {
        bool IsSceneLoaded(string sceneName);

        UniTask LoadSceneAsync(string sceneName, bool isAdditive, CancellationToken ct);

        UniTask UnloadSceneAsync(string sceneName, CancellationToken ct);
    }
}