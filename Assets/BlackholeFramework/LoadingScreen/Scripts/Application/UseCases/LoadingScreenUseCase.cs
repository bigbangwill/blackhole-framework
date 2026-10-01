using System;
using Cysharp.Threading.Tasks;
using Blackhole.LoadingScreen.Contract;

namespace Blackhole.LoadingScreen.Application
{
    public class LoadingScreenUseCase : ILoadingScreenService
    {
        private readonly LoadingScreenPresenterRegistry _presenter;

        public LoadingScreenUseCase(LoadingScreenPresenterRegistry presenter)
        {
            _presenter = presenter;
        }

        public async UniTask FadeInAsync(LoadingScreenType type)
        {
            await _presenter.FadeIn(type);
        }

        public async UniTask FadeOutAsync(LoadingScreenType type)
        {
            await _presenter.FadeOut(type);
        }

        public async UniTask<T> ExecuteAsync<T>(Func<UniTask<T>> operation, LoadingScreenType type)
        {
            await _presenter.FadeIn(type);

            try
            {
                return await operation();
            }
            finally
            {
                await _presenter.FadeOut(type);
            }
        }

        public async UniTask ExecuteAsync(Func<UniTask> operation, LoadingScreenType type)
        {
            await _presenter.FadeIn(type);

            try
            {
                await operation();
            }
            finally
            {
                await _presenter.FadeOut(type);
            }
        }
    }
}
