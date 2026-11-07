using System;
using System.Threading;
using Blackhole.SceneLoader.Contract;
using Cysharp.Threading.Tasks;
using VContainer.Unity;

namespace Blackhole.ModuleSystem.Composition
{
    public sealed class ModuleGraphStartup : IAsyncStartable
    {
        private readonly LifetimeScope scope;
        private readonly ResolverDto graph;
        private readonly ISceneTransitionUseCase sceneLoader;
        private readonly UniTaskCompletionSource ready = new();
        private bool started;

        public ModuleGraphStartup(LifetimeScope scope, ResolverDto graph, ISceneTransitionUseCase sceneLoader)
        {
            this.scope = scope;
            this.graph = graph;
            this.sceneLoader = sceneLoader;
        }

        public UniTask WaitUntilReadyAsync(CancellationToken cancellationToken)
        {
            return ready.Task.AttachExternalCancellation(cancellationToken);
        }

        public async UniTask StartAsync(CancellationToken cancellationToken)
        {
            if (started)
            {
                await WaitUntilReadyAsync(cancellationToken);
                return;
            }

            started = true;

            try
            {
                foreach (string scenePath in graph.Scenes)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (scenePath == scope.gameObject.scene.path)
                        continue;

                    using (LifetimeScope.EnqueueParent(scope))
                    {
                        await sceneLoader.LoadSceneAsync(scenePath, true, cancellationToken);
                    }
                }

                cancellationToken.ThrowIfCancellationRequested();
                ready.TrySetResult();
            }
            catch (OperationCanceledException exception)
            {
                ready.TrySetCanceled(exception.CancellationToken);
                throw;
            }
            catch (Exception exception)
            {
                ready.TrySetException(exception);
                throw;
            }
        }
    }
}
