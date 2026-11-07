using System.Collections;
using System.Threading;
using Blackhole.Framework.Composition;
using Blackhole.ModuleSystem.Composition;
using Blackhole.ModuleSystem.Contract;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using VContainer;
using VContainer.Unity;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Blackhole.Development.Tests
{
    public sealed class FrameworkStartupTests
    {
        [UnityTest]
        public IEnumerator FrameworkStartsConfiguredPersistentRootModule()
        {
            yield return SceneManager.LoadSceneAsync(FrameworkLifetimeScope.FrameworkScenePath);
            FrameworkLifetimeScope scope = Object.FindFirstObjectByType<FrameworkLifetimeScope>();
            var settings = scope.Container.Resolve<BlackholeProjectSettingsSO>();
            string path = settings.StartingModule.ModuleScene;
            float deadline = Time.realtimeSinceStartup + 20;
            while (SceneManager.GetActiveScene().path != path && Time.realtimeSinceStartup < deadline) yield return null;
            var scene = SceneManager.GetSceneByPath(path);
            Assert.That(settings.StartingModule.IsPersistant, Is.True);
            Assert.That(scene.IsValid() && scene.isLoaded, Is.True);
            Assert.That(SceneManager.GetActiveScene(), Is.EqualTo(scene));
            EventSystem[] eventSystems = Object.FindObjectsByType<EventSystem>();
            Assert.That(eventSystems.Length, Is.EqualTo(1));
            EventSystem sharedEventSystem = eventSystems[0];
            Assert.That(sharedEventSystem.gameObject.scene.path, Is.EqualTo(FrameworkLifetimeScope.FrameworkScenePath));
            Assert.That(sharedEventSystem.GetComponent<InputSystemUIInputModule>().actionsAsset, Is.Not.Null);

            IModuleLoader loader = scope.Container.Resolve<IModuleLoader>();
            yield return loader.UnloadCurrentRootModuleAsync(CancellationToken.None).ToCoroutine();
            Assert.That(EventSystem.current, Is.SameAs(sharedEventSystem));
            Assert.That(SceneManager.GetSceneByPath(path).isLoaded, Is.False);
            yield return loader.LoadRootModuleAsync(settings.StartingModule.ModuleID, CancellationToken.None).ToCoroutine();
            Assert.That(EventSystem.current, Is.SameAs(sharedEventSystem));
            Assert.That(Object.FindObjectsByType<EventSystem>().Length, Is.EqualTo(1));
            Assert.That(SceneManager.GetActiveScene().path, Is.EqualTo(path));
        }

#if UNITY_EDITOR
        [UnityTest]
        public IEnumerator FrameworkDependencySceneIsReadyBeforeStartingRootAndSurvivesRootUnload()
        {
            const string frameworkManifestPath = "Assets/BlackholeFramework/Framework/Blackhole.Framework.asset";
            const string loadingManifestPath = "Assets/BlackholeFramework/LoadingScreen/Blackhole.LoadingScreen.asset";
            var frameworkManifest = AssetDatabase.LoadAssetAtPath<ModuleManifestSO>(frameworkManifestPath);
            var loadingManifest = AssetDatabase.LoadAssetAtPath<ModuleManifestSO>(loadingManifestPath);
            Assert.That(frameworkManifest.Dependencies, Has.No.Member(loadingManifest));

            bool wasDirty = EditorUtility.IsDirty(frameworkManifest);
            var serializedManifest = new SerializedObject(frameworkManifest);
            var dependencies = serializedManifest.FindProperty("dependencies");
            int dependencyCount = dependencies.arraySize;
            dependencies.arraySize++;
            dependencies.GetArrayElementAtIndex(dependencyCount).objectReferenceValue = loadingManifest;
            serializedManifest.ApplyModifiedPropertiesWithoutUndo();
            try
            {
                yield return SceneManager.LoadSceneAsync(FrameworkLifetimeScope.FrameworkScenePath);
            }
            finally
            {
                serializedManifest.Update();
                dependencies.arraySize = dependencyCount;
                serializedManifest.ApplyModifiedPropertiesWithoutUndo();
                if (!wasDirty) EditorUtility.ClearDirty(frameworkManifest);
            }

            var scope = Object.FindFirstObjectByType<FrameworkLifetimeScope>();
            string startingPath = scope.Container.Resolve<BlackholeProjectSettingsSO>().StartingModule.ModuleScene;
            float deadline = Time.realtimeSinceStartup + 20;
            while (SceneManager.GetActiveScene().path != startingPath && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(SceneManager.GetActiveScene().path, Is.EqualTo(startingPath));

            Scene dependencyScene = SceneManager.GetSceneByPath(loadingManifest.ModuleScene);
            Assert.That(dependencyScene.isLoaded, Is.True);
            var dependencyScope = System.Array.Find(Object.FindObjectsByType<LifetimeScope>(), candidate => candidate.gameObject.scene == dependencyScene);
            Assert.That(dependencyScope?.Parent, Is.SameAs(scope));

            var loader = scope.Container.Resolve<IModuleLoader>();
            yield return loader.UnloadCurrentRootModuleAsync(CancellationToken.None).ToCoroutine();
            Assert.That(dependencyScene.isLoaded, Is.True);
        }
#endif
    }
}
