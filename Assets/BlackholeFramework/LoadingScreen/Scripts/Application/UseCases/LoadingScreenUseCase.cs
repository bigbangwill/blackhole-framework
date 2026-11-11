using System;
using Cysharp.Threading.Tasks;
using Blackhole.LoadingScreen.Contract;

namespace Blackhole.LoadingScreen.Application
{
    public class LoadingScreenUseCase : ILoadingScreenService
    {
        private readonly LoadingScreenPresenterRegistry _presenter;
        private int _transitionOperations;
        private int _manualRequest;
        private bool _manualTransition;

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

        public async UniTask OpenTransitionAsync()
        {
            int request = ++_manualRequest;
            _manualTransition = true;
            try
            {
                await _presenter.OpenTransitionAsync();
            }
            catch
            {
                if (request == _manualRequest)
                    _manualTransition = false;
                throw;
            }
        }

        public UniTask CloseTransitionAsync()
        {
            _manualRequest++;
            _manualTransition = false;
            return CloseIfIdle();
        }

        public async UniTask ExecuteTransitionAsync(Func<UniTask> operation)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            _transitionOperations++;
            try
            {
                await _presenter.OpenTransitionAsync();
                await operation();
            }
            finally
            {
                await UniTask.SwitchToMainThread();
                _transitionOperations--;
                await CloseIfIdle();
            }
        }

        private UniTask CloseIfIdle() => _transitionOperations == 0 && !_manualTransition
            ? _presenter.CloseTransitionAsync()
            : UniTask.CompletedTask;
    }
}
