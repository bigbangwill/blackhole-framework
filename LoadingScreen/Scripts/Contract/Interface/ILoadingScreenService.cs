using Cysharp.Threading.Tasks;
using System;

namespace Blackhole.LoadingScreen.Contract
{
    public interface ILoadingScreenService
    {
        UniTask FadeInAsync(LoadingScreenType type);
        UniTask FadeOutAsync(LoadingScreenType type);

        UniTask<T> ExecuteAsync<T>(Func<UniTask<T>> operation, LoadingScreenType type);
        UniTask ExecuteAsync(Func<UniTask> operation, LoadingScreenType type);
    }
}