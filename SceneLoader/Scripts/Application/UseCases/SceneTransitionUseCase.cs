using Blackhole.SceneLoader.Contract;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;

namespace Blackhole.SceneLoader.Application
{
    public sealed class SceneTransitionUseCase : ISceneTransitionUseCase, IDisposable
    {
        private readonly ISceneLoaderPort sceneLoaderPort;
        private readonly SemaphoreSlim gate = new(1, 1);

        public SceneTransitionUseCase(ISceneLoaderPort sceneLoaderPort)
        {
            this.sceneLoaderPort = sceneLoaderPort;
        }

        public async UniTask LoadSceneAsync(string sceneName, bool isAdditive, CancellationToken ct)
        {
            await gate.WaitAsync(ct);

            try
            {
                if (sceneLoaderPort.IsSceneLoaded(sceneName))
                    return;

                if (!isAdditive)
                {
                    // Later:
                    // await loadingScreen.ShowAsync(ct);
                }

                await sceneLoaderPort.LoadSceneAsync(sceneName, isAdditive, ct);

                if (!isAdditive)
                {
                    // Later:
                    // await loadingScreen.HideAsync(ct);
                }
            }
            finally
            {
                gate.Release();
            }
        }

        public async UniTask UnloadSceneAsync(string sceneName, CancellationToken ct)
        {
            await gate.WaitAsync(ct);

            try
            {
                if (!sceneLoaderPort.IsSceneLoaded(sceneName))
                    return;

                await sceneLoaderPort.UnloadSceneAsync(sceneName, ct);
            }
            finally
            {
                gate.Release();
            }
        }

        public void Dispose()
        {
            gate.Dispose();
        }
    }
}