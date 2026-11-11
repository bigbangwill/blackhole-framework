using System;
using Blackhole.LoadingScreen.Contract;
using Cysharp.Threading.Tasks;

namespace Blackhole.LoadingScreen.Application
{
    public sealed class LoadingScreenPresenterRegistry : ILoadingScreenPresenter
    {
        private ILoadingScreenPresenter _presenter;

        public void Attach(ILoadingScreenPresenter presenter)
        {
            _presenter = presenter ?? throw new ArgumentNullException(nameof(presenter));
        }

        public UniTask FadeIn(LoadingScreenType type) => GetPresenter().FadeIn(type);

        public UniTask FadeOut(LoadingScreenType type) => GetPresenter().FadeOut(type);

        public UniTask OpenTransitionAsync() => GetPresenter().OpenTransitionAsync();

        public UniTask CloseTransitionAsync() => CurrentPresenter?.CloseTransitionAsync() ?? UniTask.CompletedTask;

        private ILoadingScreenPresenter GetPresenter() => CurrentPresenter
            ?? throw new InvalidOperationException("The project loading scene has not registered its presenter.");

        private ILoadingScreenPresenter CurrentPresenter
        {
            get
            {
                if (_presenter is UnityEngine.Object unityObject && unityObject == null)
                    _presenter = null;

                return _presenter;
            }
        }
    }
}
