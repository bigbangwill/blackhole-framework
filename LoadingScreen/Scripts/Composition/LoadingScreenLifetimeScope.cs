using Blackhole.LoadingScreen.Application;
using Blackhole.LoadingScreen.Presentation;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Blackhole.LoadingScreen.Composition
{
    public class LoadingScreenLifetimeScope : LifetimeScope
    {
        [SerializeField] private LoadingScreenPresenter _loadingScreenPresenter;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(_loadingScreenPresenter).As<ILoadingScreenPresenter>();
            builder.RegisterBuildCallback(resolver => resolver.Resolve<LoadingScreenPresenterRegistry>().Attach(_loadingScreenPresenter));
        }
    }
}
