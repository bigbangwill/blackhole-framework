using Blackhole.ModuleSystem.Composition;
using Blackhole.SceneLoader.Contract;
using Blackhole.SharedKernel.Composition;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Blackhole.Multiplayer.Testing
{
    public sealed class MultiplayerTestLifetimeScope : LifetimeScope
    {
        [SerializeField] private ModuleManifestCatalogSO _catalog;
        [SerializeField] private ModuleManifestSO _module;

        private ResolverDto _graph;

        protected override void Configure(IContainerBuilder builder)
        {
            ScopeCancellationInstaller.Install(builder);
            _graph = ModuleGraphBootstrapper.Install(builder, _module.ModuleID, _catalog);
        }

        private void Start()
        {
            LoadDependencyScenesAsync().Forget();
        }

        private async UniTask LoadDependencyScenesAsync()
        {
            var sceneLoader = Container.Resolve<ISceneTransitionUseCase>();
            var cancellationToken = this.GetCancellationTokenOnDestroy();

            foreach (string scenePath in _graph.Scenes)
            {
                if (scenePath == gameObject.scene.path)
                    continue;

                using (LifetimeScope.EnqueueParent(this))
                {
                    await sceneLoader.LoadSceneAsync(scenePath, true, cancellationToken);
                }
            }

            Debug.Log($"[{_module.ModuleID}] Dependency installers and scenes are ready.");
        }
    }
}
