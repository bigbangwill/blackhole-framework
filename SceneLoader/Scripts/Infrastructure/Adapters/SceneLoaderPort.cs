using Blackhole.SceneLoader.Application;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine.SceneManagement;

namespace Blackhole.SceneLoader.Infrastructure
{
    public sealed class UnitySceneLoaderPort : ISceneLoaderPort
    {
        public bool IsSceneLoaded(string sceneName)
        {
            Scene scene = GetScene(sceneName);
            return scene.IsValid() && scene.isLoaded;
        }

        public async UniTask LoadSceneAsync(string sceneName, bool isAdditive, CancellationToken ct)
        {
            var mode = isAdditive ? LoadSceneMode.Additive : LoadSceneMode.Single;
            await SceneManager.LoadSceneAsync(sceneName, mode).ToUniTask(cancellationToken: ct);
        }

        public async UniTask UnloadSceneAsync(string sceneName, CancellationToken ct)
        {
            Scene scene = GetScene(sceneName);
            if (!scene.IsValid() || !scene.isLoaded)
                return;

            await SceneManager.UnloadSceneAsync(scene).ToUniTask(cancellationToken: ct);
        }

        private static Scene GetScene(string sceneNameOrPath)
        {
            Scene scene = SceneManager.GetSceneByPath(sceneNameOrPath);
            return scene.IsValid() ? scene : SceneManager.GetSceneByName(sceneNameOrPath);
        }
    }
}
